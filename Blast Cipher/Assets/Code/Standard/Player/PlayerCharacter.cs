using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Experimental.Input.Haptics;
using UnityEngine.Experimental.Input.Plugins.Users;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public abstract class InputSystemMonoBehaviour : MonoBehaviour
{
	public bool CanBeTeleported = true;

	private void OnEnable() => RegisterActions();
	private void OnDisable() => UnregisterActions();

	protected abstract void RegisterActions();
	protected abstract void UnregisterActions();
}

public class PlayerCharacter : InputSystemMonoBehaviour
{
	public enum ActionType { Undefined, Shoot, Dodge }
	public delegate void Hook(ActionType action);

	#region Fields
	[SerializeField] private PlayerCharacterSettings settings;

	[Space]
	public bool DebugKBControlsActive = false;
	
	[Space]
	[SerializeField] private Animator parryAnimator;
	[SerializeField] private Transform projectileOrbitCenter;
	[SerializeField] private Transform projectileLaunchPos;
	[SerializeField] private GameObject projectilePrefab;
	[SerializeField] private ParticleSystem afterImageParticleSystem;
	[SerializeField] private LineRenderer aimLineRenderer;

	[NonSerialized] public CharacterController CharController;
	[NonSerialized] public Vector2 MovementInput;
	[NonSerialized] public Vector2 AimInput;
	[NonSerialized] public bool NetworkControlled = false;

	[NonSerialized] public readonly Dictionary<PowerUpType, float> activePowerUps = new Dictionary<PowerUpType, float>();
	private readonly List<Projectile> loadedProjectiles = new List<Projectile>();
	private readonly List<PowerUpType> bufferedKeys = new List<PowerUpType>();

	private GameManager gameManager;
	private CamShakeManager camShakeManager;
	private Hook networkHook;

	private int playerID;
	private Quaternion currentCoreRotation = Quaternion.identity;
	private Quaternion coreRotationDelta = Quaternion.identity;
	private Portal portalOne;
	private Portal portalTwo;
	private float currentMovespeed;
	private float currentShotCooldown = 0f;
	private float currentParryCooldown = 0f;
	private float currentDashCooldown = 0f;

	private PlayerCharacter aimLockTarget;
	private bool aimLocked = false;
	private bool aimLockInputBlocked = false; //               \/
	private bool providingAimLockInputThisFrame = false;// these two are necessary due to a current bug in the input system during simultaneous inputs on stick presses
	#endregion

	private void Awake()
	{
		camShakeManager = CamShakeManager.Instance;
		gameManager = GameManager.Instance;
		playerID = gameManager.RegisterPlayerCharacter(this);
		
		CharController = GetComponent<CharacterController>();
	}

	private void Start()
	{
		Initialize();
	}

	private void Update()
	{
		if (!gameManager.playerInputsActive) return;

		if (DebugKBControlsActive) DebugKeyboardInput();

		UpdateMovement();
		UpdateLookRotation();

		UpdateProjectileOrbit();

		UpdateMiscValues();
	}

	private void OnDestroy()
	{
		gameManager.UnregisterPlayerCharacter(this);
	}

	#region Setup
	protected override void RegisterActions()
	{
		if (NetworkControlled) return;

		settings.InputMaster.Player.Movement.performed += UpdateMovementControlled;
		settings.InputMaster.Player.Aim.performed += UpdateLookRotationControlled;
		settings.InputMaster.Player.Shoot.performed += TriggerShotControlled;
		settings.InputMaster.Player.Jump.performed += TriggerDash;
		settings.InputMaster.Player.Parry.performed += TriggerParry;
		settings.InputMaster.Player.LockAim.performed += TriggerAimLock;
		settings.InputMaster.Player.PortalOne.performed += TriggerPortalOne;
		settings.InputMaster.Player.PortalTwo.performed += TriggerPortalTwo;

		settings.InputMaster.Player.Debug.performed += TriggerDebugAction;
	}

	protected override void UnregisterActions()
	{
		if (NetworkControlled) return;

		settings.InputMaster.Player.Movement.performed -= UpdateMovementControlled;
		settings.InputMaster.Player.Aim.performed -= UpdateLookRotationControlled;
		settings.InputMaster.Player.Shoot.performed -= TriggerShotControlled;
		settings.InputMaster.Player.Jump.performed -= TriggerDash;
		settings.InputMaster.Player.Parry.performed -= TriggerParry;
		settings.InputMaster.Player.LockAim.performed -= TriggerAimLock;
		settings.InputMaster.Player.PortalOne.performed -= TriggerPortalOne;
		settings.InputMaster.Player.PortalTwo.performed -= TriggerPortalTwo;

		settings.InputMaster.Player.Debug.performed -= TriggerDebugAction;
	}

	public void Initialize()
	{
		currentMovespeed = settings.MovespeedMod;

		// convert core rotation v3 to quaternion
		coreRotationDelta = Quaternion.Euler(settings.OrbitDelta);

		// since its only 1o1, this only needs to be called once
		aimLockTarget = gameManager.RequestNearestPlayer(this);

		// spawn initial projectile
		PickupProjectile(Instantiate(projectilePrefab).GetComponent<Projectile>());
	}
	#endregion

	#region InputSystem event calls
	private bool IsAssignedDevice(InputDevice controller)
	{
		if (playerID >= gameManager.inputDevices.Length) return false;
		return gameManager.inputDevices[playerID] == controller;
	}

	private void DebugKeyboardInput()
	{
		MovementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

		if (Input.GetKeyDown(KeyCode.E) && loadedProjectiles.Count > 0)
		{
			Shoot();
		}

		if(Input.GetKeyDown(KeyCode.Q))
		{
			parryAnimator.SetTrigger("ConstructParryShield");
			currentParryCooldown = settings.ParryCooldown;
		}

		if (Input.GetKeyDown(KeyCode.R))
		{
			currentShotCooldown = settings.ShotCooldown;
			PickupProjectile(Instantiate(projectilePrefab).GetComponent<Projectile>());
		}

		if(Input.GetKeyDown(KeyCode.F))
		{
			aimLockTarget = gameManager.RequestNearestPlayer(this);
			aimLocked = !aimLocked;
			aimLockInputBlocked = true;
		}

		if (Input.GetKeyDown(KeyCode.Y)) CreatePortal(0);
		else if (Input.GetKeyDown(KeyCode.X)) CreatePortal(1);

		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (currentDashCooldown == 0f && CharController.velocity.sqrMagnitude > .1f)
			{
				currentDashCooldown = settings.DashCooldown;
				StartCoroutine(DashSequence());
			}
		}
	}

	private void UpdateMovementControlled(InputAction.CallbackContext ctx)
	{
		if (IsAssignedDevice(ctx.control.device))
		{
			MovementInput = ctx.ReadValue<Vector2>();
		}
	}

	private void UpdateLookRotationControlled(InputAction.CallbackContext ctx)
	{
		if (IsAssignedDevice(ctx.control.device)) AimInput = ctx.ReadValue<Vector2>();
	}

	private void TriggerShotControlled(InputAction.CallbackContext ctx)
	{
		if (currentShotCooldown == 0f && loadedProjectiles.Count > 0 && IsAssignedDevice(ctx.control.device))
		{
			Shoot();
		}
	}

	private void TriggerDash(InputAction.CallbackContext ctx)
	{
		if (IsAssignedDevice(ctx.control.device)
			&& currentDashCooldown == 0f 
			&& CharController.velocity.sqrMagnitude > .1f)
		{
			currentDashCooldown = settings.DashCooldown;
			StartCoroutine(DashSequence());
		}
	}

	private void TriggerParry(InputAction.CallbackContext ctx)
	{
		if (currentParryCooldown == 0f && IsAssignedDevice(ctx.control.device))
		{
			parryAnimator.SetTrigger("ConstructParryShield");
			currentParryCooldown = settings.ParryCooldown;
		}
	}

	private void TriggerAimLock(InputAction.CallbackContext ctx)
	{
		if (IsAssignedDevice(ctx.control.device))
		{
			providingAimLockInputThisFrame = true;
			if (!aimLockInputBlocked)
			{
				aimLockTarget = gameManager.RequestNearestPlayer(this);
				//Debug.Log(aimLockTarget.name);
				aimLocked = !aimLocked;
				aimLockInputBlocked = true;
			}
		}
	}

	private void TriggerPortalOne(InputAction.CallbackContext ctx)
	{
		if (IsAssignedDevice(ctx.control.device)) CreatePortal(0);
	}

	private void TriggerPortalTwo(InputAction.CallbackContext ctx)
	{
		if (IsAssignedDevice(ctx.control.device)) CreatePortal(1);
	}

	private void TriggerDebugAction(InputAction.CallbackContext ctx)
	{
		if (IsAssignedDevice(ctx.control.device) && currentShotCooldown == 0f)
		{
			currentShotCooldown = settings.ShotCooldown;
			PickupProjectile(Instantiate(projectilePrefab).GetComponent<Projectile>());
		}
	}
	#endregion

	#region Regular Updates
	private void UpdateMovement()
	{
		Camera mainCam = CamMover.Instance.Cam;
		if (mainCam == null)
		{
			Debug.Log("main cam missing");
			return;
		}

		var camHorizontal = new Vector3(mainCam.transform.right.x, 0f, mainCam.transform.right.z).normalized;
		var camVertical = new Vector3(mainCam.transform.forward.x, 0f, mainCam.transform.forward.z).normalized;

		float mod = Time.deltaTime * currentMovespeed;
		var movement = Vector3.ClampMagnitude
			(camHorizontal * MovementInput.x
			+ camVertical * MovementInput.y, 1f) * mod; // move player relative to camera

		movement.y = CharController.isGrounded ? 0f : Physics.gravity.y * Time.deltaTime;

		CharController.Move(movement);
	}

	private void UpdateLookRotation()
	{
		Vector3 lookDir;
		if (aimLocked)
		{
			if (aimLockTarget == null)
			{
				aimLocked = false;
				return;
			}

			lookDir = (aimLockTarget.transform.position - transform.position).normalized;
			lookDir.y = 0f;
			aimLineRenderer.SetPosition(1, new Vector3(0f, 0f, settings.AimLineLengthMax));
		}
		else if (AimInput.sqrMagnitude < .1f)
		{
			aimLineRenderer.SetPosition(1, new Vector3(0f, 0f, 1f));
			return;
		}
		else
		{
			lookDir = new Vector3(AimInput.x, 0f, AimInput.y);
			aimLineRenderer.SetPosition(1, new Vector3(0f, 0f, AimInput.magnitude * settings.AimLineLengthMax));
		}


		transform.rotation = Quaternion.LookRotation(lookDir, transform.up);
	}

	private void UpdateProjectileOrbit()
	{
		// calc current core rotation (a.k.a rotation that applies to all projectiles onto their individual ones)
		currentCoreRotation = Quaternion.LerpUnclamped(currentCoreRotation, currentCoreRotation * coreRotationDelta, Time.deltaTime);

		Projectile projectile;
		Vector3 targetPos;
		for (int i = 0; i < loadedProjectiles.Count; i++)
		{
			projectile = loadedProjectiles[i];

			Quaternion selfRotation = 
				Quaternion.Euler(new Vector3(0f, (360 / loadedProjectiles.Count) * i, 0f)) 
				* currentCoreRotation; // combine partial rotation with core rotation for this particles rotation around the orbit center
			Vector3 orbitVec = Vector3.forward * settings.OrbitDist; // identity vector for all projectiles
			orbitVec = selfRotation * orbitVec; // vector from orbit center to target position (local space)
			targetPos = projectileOrbitCenter.position + orbitVec;

			if (MovementInput.sqrMagnitude > .5f)
			{
				Vector3 targetPosTwo = transform.position + transform.up * settings.MovementOrbit;
				targetPos = Vector3.Lerp(targetPos, targetPosTwo, settings.MovementInterpolation);
			}

			projectile.transform.position = Vector3.MoveTowards
				(projectile.transform.position, 
				targetPos, 
				Vector3.Distance(projectile.transform.position, targetPos) * settings.MaxOrbitDist * Time.deltaTime);

			UpdateProjectileRotation(projectile);
		}
	}

	private void UpdateProjectileRotation(Projectile projectile)
	{
		projectile.transform.rotation *= 
			Quaternion.Euler
			(Random.Range(settings.RotationMin, settings.RotationMax) * Time.deltaTime, 
			Random.Range(settings.RotationMin, settings.RotationMax) * Time.deltaTime, 
			Random.Range(settings.RotationMin, settings.RotationMax) * Time.deltaTime);
	}
	
	private void UpdateMiscValues()
	{
		if (!providingAimLockInputThisFrame && aimLockInputBlocked) aimLockInputBlocked = false;
		providingAimLockInputThisFrame = false;

		Utilities.CountDownVal(ref currentShotCooldown);
		Utilities.CountDownVal(ref currentParryCooldown);
		Utilities.CountDownVal(ref currentDashCooldown);

		if(activePowerUps.Count > 0)
		{
			bufferedKeys.AddRange(activePowerUps.Keys);
			PowerUpType key;
			float val;
			for (int i = 0; i < bufferedKeys.Count; i++)
			{
				key = bufferedKeys[i];
				val = Utilities.CountDownVal(activePowerUps[key]);

				if (val == 0f)
				{
					RemovePowerUp(key, loadedProjectiles);
					activePowerUps.Remove(key);
				}
				else activePowerUps[key] = val;
			}
			bufferedKeys.Clear();
		}
	}
	#endregion

	#region Actions
	private void Shoot()
	{
		camShakeManager.ShakeMagnitude = settings.ShotShakeMagnitude;
		currentShotCooldown = settings.ShotCooldown;

		var projectile = loadedProjectiles[0];
		loadedProjectiles.Remove(projectile);
		Shoot(projectile);

		networkHook?.Invoke(ActionType.Shoot);
	}

	private void Shoot(Projectile projectile)
	{
		projectile.transform.position = projectileLaunchPos.transform.position;
		SetProjectileEnabled(projectile, true);

		// launch projectile in aim direction
		var forceVec = projectileLaunchPos.position - transform.position;
		forceVec.y = 0f;

		Vector3 shotVec = forceVec.normalized * settings.ShotStrength;
		projectile.rgb.AddForce(shotVec, ForceMode.Impulse);
		projectile.actualVelocity = shotVec; // for portal/wall bounces

		projectile.InitialShooter = this;

		projectile = null;

		OneShotAudioManager.PlayOneShotAudio(settings.ProjectileShotSounds, projectileLaunchPos.position);
	}

	public void TriggerDeath()
	{
		Debug.Log(name + " died");
		for (int i = 0; i < loadedProjectiles.Count; i++)
		{
			SetProjectileEnabled(loadedProjectiles[i], true);
			loadedProjectiles[i].rgb.constraints = RigidbodyConstraints.None;
			loadedProjectiles[i].CanPickup = true;
		}

		camShakeManager.ShakeMagnitude = settings.DeathShakeMagnitude;
		OneShotAudioManager.PlayOneShotAudio(settings.PlayerDeathSounds, transform.position);
		gameManager.StartNextRound();
		Destroy(gameObject);
	}

	public void PickupProjectile(Projectile projectile)
	{
		loadedProjectiles.Add(projectile);
		SetProjectileEnabled(projectile, false);
		projectile.CanPickup = false;
		ApplyPowerUp(activePowerUps.Keys, projectile);
	}

	private void SetProjectileEnabled(Projectile projectile, bool enableState)
	{
		if (enableState) projectile.rgb.WakeUp();
		else projectile.rgb.Sleep();

		projectile.rgb.useGravity = enableState;
		projectile.collider.enabled = enableState;
	}

	public IEnumerator DashSequence()
	{
		var main = afterImageParticleSystem.main;
		main.duration = settings.DashDuration + settings.DashAfterimagePadding;
		afterImageParticleSystem.Play();

		float baseSpeed = currentMovespeed;
		currentMovespeed = settings.DashSpeed;

		yield return new WaitForSeconds(settings.DashDuration);
		currentMovespeed = baseSpeed;
	}

	public void CreatePortal(int portalID)
	{
		if (!Physics.Raycast(projectileLaunchPos.position, transform.forward, out var hit, 100f, settings.TeleportCompatibleLayers)) return;
		float yRot;
		Quaternion rotation;

		switch (portalID)
		{
			case 0:
				if (portalOne != null)
				{
					if (portalOne.lineRenderer != null) Destroy(portalOne.lineRenderer.gameObject);
					Destroy(portalOne.gameObject);
				}

				yRot = Vector3.Angle(Vector3.right, hit.normal);
				rotation = Quaternion.Euler(0f, yRot, 90f);
				portalOne = Instantiate(settings.PortalPrefab, hit.point, rotation);

				if (portalTwo != null) portalTwo.ConnectPortals(portalOne);
				break;

			case 1:
				if (portalTwo != null)
				{
					if (portalTwo.lineRenderer != null) Destroy(portalTwo.lineRenderer.gameObject);
					Destroy(portalTwo.gameObject);
				}

				yRot = Vector3.Angle(Vector3.right, hit.normal);
				rotation = Quaternion.Euler(0f, yRot, 90f);
				portalTwo = Instantiate(settings.PortalPrefab, hit.point, rotation);

				if (portalOne != null) portalOne.ConnectPortals(portalTwo);
				break;
		}
	}
	#endregion

	#region Network Specific
	public void RegisterNetworkHook(Hook hook)
	{
		networkHook = hook;
	}

	public void PerformAction(ActionType action)
	{
		switch (action)
		{
			case ActionType.Shoot:
				if (loadedProjectiles.Count > 0) Shoot();
				break;
		}
	}
	#endregion

	#region PowerUpHandling
	public void OnPowerUpCollect(PowerUpType type)
	{
		if (activePowerUps.ContainsKey(type)) activePowerUps[type] += settings.PowerUpDuration;
		else
		{
			activePowerUps.Add(type, settings.PowerUpDuration);
			ApplyPowerUp(type, loadedProjectiles);
		}
	}

	private void ApplyPowerUp(PowerUpType powerUp, Projectile projectile)
	{
		switch (powerUp)
		{
			case PowerUpType.Bomb:
				projectile.Explosive = true;
				break;

			case PowerUpType.Bounce:
				projectile.Bounces = 2;
				break;

			case PowerUpType.AutoAim:
				projectile.ExplicitTarget = gameManager.RequestNearestPlayer(this);
				break;
		}
	}

	private void ApplyPowerUp(PowerUpType powerUp, List<Projectile> projectiles)
	{
		for (int i = 0; i < projectiles.Count; i++) ApplyPowerUp(powerUp, projectiles[i]);
	}

	private void ApplyPowerUp(Dictionary<PowerUpType, float>.KeyCollection powerUps, Projectile projectile)
	{
		foreach (var key in powerUps) ApplyPowerUp(key, projectile);
	}

	private void RemovePowerUp(PowerUpType powerUp, Projectile projectile)
	{
		switch (powerUp)
		{
			case PowerUpType.Bomb:
				projectile.Explosive = false;
				break;

			case PowerUpType.Bounce:
				projectile.Bounces = 0;
				break;

			case PowerUpType.AutoAim:
				projectile.ExplicitTarget = null;
				break;
		}
	}
	
	private void RemovePowerUp(PowerUpType powerUp, List<Projectile> projectiles)
	{
		for (int i = 0; i < projectiles.Count; i++) RemovePowerUp(powerUp, projectiles[i]);
	}

	private void RemovePowerUp(Dictionary<PowerUpType, float>.KeyCollection powerUps, Projectile projectile)
	{
		foreach (var key in powerUps) RemovePowerUp(key, projectile);
	}
	#endregion

	private void OnControllerColliderHit(ControllerColliderHit hit) // pickup logic
	{
		if(hit.collider.CompareTag("Projectile"))
		{
			var projectile = hit.collider.GetComponent<Projectile>();
			if (projectile.CanPickup) PickupProjectile(projectile);
		}
		else if (hit.collider.CompareTag("PowerUp"))
		{
			OnPowerUpCollect(hit.gameObject.GetComponent<PowerUp>().type);
			Destroy(hit.gameObject);
		}
	}
}
