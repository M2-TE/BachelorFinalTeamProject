using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Experimental.Input.Haptics;
using UnityEngine.Experimental.Input.Plugins.Users;
using Random = UnityEngine.Random;

public abstract class InputSystemMonoBehaviour : MonoBehaviour
{
	private bool _canBeTeleported = true;
	public bool CanBeTeleported { get => _canBeTeleported; set => _canBeTeleported = value; }

	private void OnEnable() => RegisterActions();
	private void OnDisable() => UnregisterActions();

	protected abstract void RegisterActions();
	protected abstract void UnregisterActions();
}

public class PlayerCharacter : InputSystemMonoBehaviour
{
	#region Fields
	[SerializeField] private PlayerCharacterSettings settings;

	[Space]
	public bool DebugKBControlsActive = false;
	public bool NetworkControlled = false;
	
	[Space]
	[SerializeField] private Animator parryAnimator;
	[SerializeField] private Transform projectileOrbitCenter;
	[SerializeField] private Transform projectileLaunchPos;
	[SerializeField] private GameObject projectilePrefab;
	[SerializeField] private ParticleSystem afterImageParticleSystem;
	[SerializeField] private LineRenderer aimLineRenderer;

	public enum ActionType { Undefined, Shoot, Dodge }
	public delegate void Hook(ActionType action);
	private Hook networkHook;

	private DualMotorRumble rumble;
	private InputUser inputUser;
	private InputDevice _inputDevice;
	public InputDevice InputDevice
	{
		get => _inputDevice;
		set
		{
			_inputDevice = value;

			inputUser = InputUser.PerformPairingWithDevice(value, options: InputUserPairingOptions.UnpairCurrentDevicesFromUser);
		}
	}

	private GameManager gameManager;
	private CamShakeManager camShakeManager;
	private CharacterController charController;
	private InputMaster input;
	
	private Portal portalOne;
	private Portal portalTwo;

	[NonSerialized] public readonly Dictionary<PowerUpType, float> activePowerUps = new Dictionary<PowerUpType, float>();
	private readonly List<Projectile> loadedProjectiles = new List<Projectile>();
	private readonly List<PowerUpType> bufferedKeys = new List<PowerUpType>();

	private Vector2 movementInput;
	private Vector2 aimInput;
	private Quaternion currentCoreRotation = Quaternion.identity;
	private Quaternion coreRotationDelta = Quaternion.identity;

	private float currentMovespeed;
	private float currentShotCooldown = 0f;
	private float currentParryCooldown = 0f;
	private float currentDashCooldown = 0f;

	private PlayerCharacter aimLockTarget;
	private bool aimLocked = false;
	
	private bool aimLockInputBlocked = false; //               \/
	private bool providingAimLockInputThisFrame = false;// these two are necessary due to a current bug in the input system during simultaneous inputs on stick presses
	#endregion

	protected override void RegisterActions()
	{
		if (NetworkControlled) return;

		input.Player.Movement.performed += UpdateMovementControlled;
		input.Player.Aim.performed += UpdateLookRotationControlled;
		input.Player.Shoot.performed += TriggerShotControlled;
		input.Player.Jump.performed += TriggerDash;
		input.Player.Parry.performed += TriggerParry;
		input.Player.LockAim.performed += TriggerAimLock;
		input.Player.PortalOne.performed += TriggerPortalOne;
		input.Player.PortalTwo.performed += TriggerPortalTwo;

		input.Player.Debug.performed += TriggerDebugAction;
	}

	protected override void UnregisterActions()
	{
		if (NetworkControlled) return;

		input.Player.Movement.performed -= UpdateMovementControlled;
		input.Player.Aim.performed -= UpdateLookRotationControlled;
		input.Player.Shoot.performed -= TriggerShotControlled;
		input.Player.Jump.performed -= TriggerDash;
		input.Player.Parry.performed -= TriggerParry;
		input.Player.LockAim.performed -= TriggerAimLock;
		input.Player.PortalOne.performed -= TriggerPortalOne;
		input.Player.PortalTwo.performed -= TriggerPortalTwo;

		input.Player.Debug.performed -= TriggerDebugAction;
	}

	private void Awake()
	{
		camShakeManager = CamShakeManager.Instance;
		gameManager = GameManager.Instance;
		input = gameManager.InputMaster;
		gameManager.RegisterPlayerCharacter(this);
		
		charController = GetComponent<CharacterController>();

		InitializeFields();
	}

	private void Start()
	{
		//Initialize();
	}

	private void Update()
	{
		if (NetworkControlled) return;
		if (DebugKBControlsActive) DebugKeyboardInput();
		UpdateMovement();
		UpdateLookRotation();

		UpdateProjectileOrbit();

		UpdateMiscValues();
	}

	public void Initialize()
	{
		// since its only 1o1, this only needs to be called once
		aimLockTarget = gameManager.RequestNearestPlayer(this);

		// spawn initial projectile
		PickupProjectile(Instantiate(projectilePrefab).GetComponent<Projectile>());
	}

	public void RegisterNetworkHook(Hook hook)
	{
		networkHook = hook;
	}

	#region InputSystem event calls
	private void DebugKeyboardInput()
	{
		movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

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
			aimLocked = !aimLocked;
			aimLockInputBlocked = true;
		}

		if (Input.GetKeyDown(KeyCode.Y)) CreatePortal(0);
		else if (Input.GetKeyDown(KeyCode.X)) CreatePortal(1);

		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (currentDashCooldown == 0f && charController.velocity.sqrMagnitude > .1f)
			{
				currentDashCooldown = settings.DashCooldown;
				StartCoroutine(DashSequence());
			}
		}
	}

	private void UpdateMovementControlled(InputAction.CallbackContext ctx)
	{
		if (_inputDevice == ctx.control.device) movementInput = ctx.ReadValue<Vector2>();
	}

	private void UpdateLookRotationControlled(InputAction.CallbackContext ctx)
	{
		if (_inputDevice == ctx.control.device) aimInput = ctx.ReadValue<Vector2>();
	}

	private void TriggerShotControlled(InputAction.CallbackContext ctx)
	{
		if (currentShotCooldown == 0f && loadedProjectiles.Count > 0 && _inputDevice == ctx.control.device)
		{
			Shoot();
		}
	}

	private void TriggerDash(InputAction.CallbackContext ctx)
	{
		if (_inputDevice == ctx.control.device
			&& currentDashCooldown == 0f 
			&& charController.velocity.sqrMagnitude > .1f)
		{
			currentDashCooldown = settings.DashCooldown;
			StartCoroutine(DashSequence());
		}
	}

	private void TriggerParry(InputAction.CallbackContext ctx)
	{
		if (currentParryCooldown == 0f && _inputDevice == ctx.control.device)
		{
			parryAnimator.SetTrigger("ConstructParryShield");
			currentParryCooldown = settings.ParryCooldown;
		}
	}

	private void TriggerAimLock(InputAction.CallbackContext ctx)
	{
		if (_inputDevice == ctx.control.device)
		{
			providingAimLockInputThisFrame = true;
			if (!aimLockInputBlocked)
			{
				aimLocked = !aimLocked;
				aimLockInputBlocked = true;
			}
		}
	}

	private void TriggerPortalOne(InputAction.CallbackContext ctx)
	{
		if (_inputDevice == ctx.control.device) CreatePortal(0);
	}

	private void TriggerPortalTwo(InputAction.CallbackContext ctx)
	{
		if (_inputDevice == ctx.control.device) CreatePortal(1);
	}

	private void TriggerDebugAction(InputAction.CallbackContext ctx)
	{
		if (_inputDevice == ctx.control.device && currentShotCooldown == 0f)
		{
			currentShotCooldown = settings.ShotCooldown;
			PickupProjectile(Instantiate(projectilePrefab).GetComponent<Projectile>());
		}
	}
	#endregion

	private void InitializeFields()
	{
		currentMovespeed = settings.MovespeedMod;

		// convert core rotation v3 to quaternion
		coreRotationDelta = Quaternion.Euler(settings.OrbitDelta);
	}

	private void UpdateMovement()
	{
		Camera mainCam = gameManager.MainCam; // dont save reference to main cam to avoid static MonoBehavior ref

		var camHorizontal = new Vector3(mainCam.transform.right.x, 0f, mainCam.transform.right.z).normalized;
		var camVertical = new Vector3(mainCam.transform.forward.x, 0f, mainCam.transform.forward.z).normalized;

		float mod = Time.deltaTime * currentMovespeed;
		var movement = Vector3.ClampMagnitude
			(camHorizontal * movementInput.x 
			+ camVertical * movementInput.y, 1f) * mod; // move player relative to camera

		movement.y = charController.isGrounded ? 0f : Physics.gravity.y * Time.deltaTime;

		charController.Move(movement);
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
		else if (aimInput.sqrMagnitude < .1f)
		{
			aimLineRenderer.SetPosition(1, new Vector3(0f, 0f, 1f));
			return;
		}
		else
		{
			lookDir = new Vector3(aimInput.x, 0f, aimInput.y);
			aimLineRenderer.SetPosition(1, new Vector3(0f, 0f, aimInput.magnitude * settings.AimLineLengthMax));
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

			if (movementInput.sqrMagnitude > .5f)
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

	public void Shoot()
	{
		camShakeManager.ShakeMagnitude = settings.ShotShakeMagnitude;
		currentShotCooldown = settings.ShotCooldown;

		var projectile = loadedProjectiles[0];
		loadedProjectiles.Remove(projectile);
		Shoot(projectile);

		networkHook(ActionType.Shoot);
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

	public void OnPowerUpCollect(PowerUpType type)
	{
		if (activePowerUps.ContainsKey(type)) activePowerUps[type] += settings.PowerUpDuration;
		else
		{
			activePowerUps.Add(type, settings.PowerUpDuration);
			ApplyPowerUp(type, loadedProjectiles);
		}
	}

	#region PowerUpHandling
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
