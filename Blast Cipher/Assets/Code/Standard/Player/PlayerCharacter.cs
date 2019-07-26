using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;
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
	public PlayerCharacterSettings Settings;

	[Space]
	public bool DebugKBControlsActive;
	
	[Space]
	[SerializeField] private Animator parryAnimator;
	[SerializeField] private Transform projectileOrbitCenter;
	[SerializeField] private Transform projectileLaunchPos;
	[SerializeField] private GameObject projectilePrefab;
	[SerializeField] private ParticleSystem afterImageParticleSystem;
	[SerializeField] private LineRenderer aimLineRenderer;
    [SerializeField] private MeshFilter bodyMesh;

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
    private int playerTeam;
	private Vector3 startPos;
	private Quaternion currentCoreRotation = Quaternion.identity;
	private Quaternion coreRotationDelta = Quaternion.identity;
	private Portal portalOne;
	private Portal portalTwo;
	private float currentMovespeed;
	private bool shooting = false;

	private PlayerCharacter aimLockTarget;
	private bool aimLocked = false;
	//private bool aimLockInputBlocked = false; //               \/
	//private bool providingAimLockInputThisFrame = false;// these two are necessary due to a current bug in the input system during simultaneous inputs on stick presses

	private float currentProjectileReloadCooldown = 0f;
	private float currentShotCooldown = 0f;
	private float currentParryCooldown = 0f;
	public float CurrentParryCooldown { get => currentParryCooldown; }
	private float currentDashCooldown = 0f;
	public float CurrentDashCooldown { get => currentDashCooldown; }
	#endregion

	private void Awake()
	{
		camShakeManager = CamShakeManager.Instance;
		gameManager = GameManager.Instance;
		
        // ist nun in setup --> hab außerdem playerTeam als private int hinzugefügt
		
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
		UpdateShooting();
		PullInProjectilesGradual();
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

		Settings.InputMaster.Player.Movement.performed += UpdateMovementControlled;
		Settings.InputMaster.Player.Aim.performed += UpdateLookRotationControlled;
		Settings.InputMaster.Player.Shoot.performed += TriggerShotControlled;
		Settings.InputMaster.Player.Jump.performed += TriggerDash;
		Settings.InputMaster.Player.Parry.performed += TriggerParry;
		Settings.InputMaster.Player.LockAim.performed += TriggerAimLock;
		Settings.InputMaster.Player.Portal.performed += TriggerPortalOne;
		Settings.InputMaster.Player.ProjectileCollect.performed += CollectProjectiles;

		Settings.InputMaster.Player.Debug.performed += TriggerDebugAction;
	}

	protected override void UnregisterActions()
	{
		if (NetworkControlled) return;

		Settings.InputMaster.Player.Movement.performed -= UpdateMovementControlled;
		Settings.InputMaster.Player.Aim.performed -= UpdateLookRotationControlled;
		Settings.InputMaster.Player.Shoot.performed -= TriggerShotControlled;
		Settings.InputMaster.Player.Jump.performed -= TriggerDash;
		Settings.InputMaster.Player.Parry.performed -= TriggerParry;
		Settings.InputMaster.Player.LockAim.performed -= TriggerAimLock;
		Settings.InputMaster.Player.Portal.performed -= TriggerPortalOne;
		Settings.InputMaster.Player.ProjectileCollect.performed -= CollectProjectiles;

		Settings.InputMaster.Player.Debug.performed -= TriggerDebugAction;
	}

	public void Initialize()
	{
        playerID = gameManager.RegisterPlayerCharacter(this);
        playerTeam = gameManager.GetTeamByPlayerID(playerID);
        bodyMesh.mesh = gameManager.GetMeshByPlayerID(playerID);
        bodyMesh.gameObject.GetComponent<MeshRenderer>().material = gameManager.GetMaterialByPlayerID(playerID);

        startPos = transform.position;

		currentMovespeed = Settings.MovespeedMod;

		// convert core rotation v3 to quaternion
		coreRotationDelta = Quaternion.Euler(Settings.OrbitDelta);

		// since its only 1o1, this only needs to be called once
		aimLockTarget = gameManager.RequestNearestPlayer(this);

		// spawn initial projectile
		var instance = GameManager.Instance;
		for(int i = 0; i < Settings.startProjectileCount; i++)
		{
			PickupProjectile(instance.SpawnObject(projectilePrefab).GetComponent<Projectile>());
		}
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

		if(Input.GetKeyDown(KeyCode.Q) && CurrentParryCooldown == 0f)
		{
			parryAnimator.SetTrigger("ConstructParryShield");
			currentParryCooldown = Settings.ParryCooldown;

			OneShotAudioManager.PlayOneShotAudio(Settings.ShieldConstructionSounds, transform.position);
		}

		if (Input.GetKeyDown(KeyCode.R))
		{
			//currentShotCooldown = Settings.ShotCooldown;
			//PickupProjectile(Instantiate(projectilePrefab).GetComponent<Projectile>());
		}

		if(Input.GetKeyDown(KeyCode.F))
		{
			aimLockTarget = gameManager.RequestNearestPlayer(this);
			aimLocked = !aimLocked;
			//aimLockInputBlocked = true;
		}

		if (Input.GetKeyDown(KeyCode.Y)) CreatePortal();

		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (currentDashCooldown == 0f && CharController.velocity.sqrMagnitude > .1f)
			{
				currentDashCooldown = Settings.DashCooldown;
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

		if (IsAssignedDevice(ctx.control.device))
		{
			var inputVal = ctx.ReadValue<float>();
			if (inputVal < 1f)
			{
				if (shooting)
				{
					//Debug.Log(name + " Stopping");
				}
				shooting = false;
			}
			else
			{
				if (!shooting)
				{
					//Debug.Log(name + " Shooting");
				}
				shooting = true;
			}
		}
	}

	private void TriggerDash(InputAction.CallbackContext ctx)
	{
		if (IsAssignedDevice(ctx.control.device)
			&& currentDashCooldown == 0f 
			&& CharController.velocity.sqrMagnitude > .1f)
		{
			currentDashCooldown = Settings.DashCooldown;
			StartCoroutine(DashSequence());
		}
	}

	private void TriggerParry(InputAction.CallbackContext ctx)
	{
		if (currentParryCooldown == 0f && IsAssignedDevice(ctx.control.device))
		{
			parryAnimator.SetTrigger("ConstructParryShield");
			currentParryCooldown = Settings.ParryCooldown;
			OneShotAudioManager.PlayOneShotAudio(Settings.ShieldConstructionSounds, transform.position);
		}
	}

	private void TriggerAimLock(InputAction.CallbackContext ctx)
	{
		if (IsAssignedDevice(ctx.control.device))
		{
			float val = ctx.ReadValue<float>();
			if(aimLockTarget == null) gameManager.RequestNearestPlayer(this);

			if(val > 0f && !aimLocked)
			{
				aimLocked = true;
			}
			else if(val == 0f)
			{
				aimLocked = false;
			}
		}
	}

	private void TriggerPortalOne(InputAction.CallbackContext ctx)
	{
		if (IsAssignedDevice(ctx.control.device)) CreatePortal();
	}

	private void CollectProjectiles(InputAction.CallbackContext ctx)
	{
		if (IsAssignedDevice(ctx.control.device)) PullInProjectilesInstant();
	}

	private void TriggerDebugAction(InputAction.CallbackContext ctx)
	{
		//if (IsAssignedDevice(ctx.control.device) && currentShotCooldown == 0f)
		//{
		//	currentShotCooldown = Settings.ShotCooldown;
		//	PickupProjectile(Instantiate(projectilePrefab).GetComponent<Projectile>());
		//}
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
			aimLineRenderer.SetPosition(1, new Vector3(0f, 0f, Settings.AimLineLengthMax));
		}
		else if (AimInput.sqrMagnitude < .1f)
		{
			aimLineRenderer.SetPosition(1, new Vector3(0f, 0f, 1f));
			return;
		}
		else
		{
			lookDir = new Vector3(AimInput.x, 0f, AimInput.y);
			aimLineRenderer.SetPosition(1, new Vector3(0f, 0f, AimInput.magnitude * Settings.AimLineLengthMax));
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
			Vector3 orbitVec = Vector3.forward * Settings.OrbitDist; // identity vector for all projectiles
			orbitVec = selfRotation * orbitVec; // vector from orbit center to target position (local space)
			targetPos = projectileOrbitCenter.position + orbitVec;

			if (MovementInput.sqrMagnitude > .5f)
			{
				Vector3 targetPosTwo = transform.position + transform.up * Settings.MovementOrbit;
				targetPos = Vector3.Lerp(targetPos, targetPosTwo, Settings.MovementInterpolation);
			}

			projectile.transform.position = Vector3.MoveTowards
				(projectile.transform.position, 
				targetPos, 
				Vector3.Distance(projectile.transform.position, targetPos) * Settings.MaxOrbitDist * Time.deltaTime);

			UpdateProjectileRotation(projectile);
		}
	}

	private void UpdateProjectileRotation(Projectile projectile)
	{
		projectile.transform.rotation *= 
			Quaternion.Euler
			(Random.Range(Settings.RotationMin, Settings.RotationMax) * Time.deltaTime, 
			Random.Range(Settings.RotationMin, Settings.RotationMax) * Time.deltaTime, 
			Random.Range(Settings.RotationMin, Settings.RotationMax) * Time.deltaTime);
	}

	private void UpdateShooting()
	{
		Utilities.CountDownVal(ref currentShotCooldown);

		if (shooting && currentShotCooldown == 0f && loadedProjectiles.Count > 0)
		{
			Shoot();
			currentShotCooldown = Settings.ShotCooldown;
		}
	}

	private void PullInProjectilesGradual()
	{
		var hits = Physics.SphereCastAll(transform.position, Settings.ProjectileMagnetRadius, Vector3.forward, 0f, Settings.ProjectileLayer);
		for(int i = 0; i < hits.Length; i++)
		{
			var projectile = hits[i].collider.GetComponent<Projectile>();
			if (projectile.CanPickup)
			{
				hits[i].rigidbody.AddExplosionForce
					(-Settings.ProjectileMagnetForce,
					transform.position,
					Settings.ProjectileMagnetRadius, 
					0f,
					ForceMode.Force);

				if(Vector3.Distance(projectile.transform.position, transform.position) < Settings.ProjectileCollectionRadius)
				{
					PickupProjectile(projectile);
				}
			}
		}
	}
	
	private void UpdateMiscValues()
	{
		//if (!providingAimLockInputThisFrame && aimLockInputBlocked) aimLockInputBlocked = false;
		//providingAimLockInputThisFrame = false;

		Utilities.CountDownVal(ref currentParryCooldown);
		Utilities.CountDownVal(ref currentDashCooldown);
		Utilities.CountDownVal(ref currentProjectileReloadCooldown);

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

		if(currentProjectileReloadCooldown <= 0f)
		{
			PickupProjectile(GameManager.Instance.SpawnObject(projectilePrefab).GetComponent<Projectile>());
			currentProjectileReloadCooldown = Settings.projectileRespawnTimer;
		}
	}
	#endregion

	#region Actions
	private void Shoot()
	{
		camShakeManager.ShakeMagnitude = Settings.ShotShakeMagnitude;
		currentShotCooldown = Settings.ShotCooldown;

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

		Vector3 shotVec = forceVec.normalized * Settings.ShotStrength;
		projectile.rgb.AddForce(shotVec, ForceMode.Impulse);
		projectile.actualVelocity = shotVec; // for portal/wall bounces

		projectile.InitialShooter = this;

		projectile = null;

		OneShotAudioManager.PlayOneShotAudio(Settings.ProjectileShotSounds, projectileLaunchPos.position);
	}

	public void TriggerDeath()
	{
		for (int i = 0; i < loadedProjectiles.Count; i++)
		{
			SetProjectileEnabled(loadedProjectiles[i], true);
			loadedProjectiles[i].rgb.constraints = RigidbodyConstraints.None;
			loadedProjectiles[i].CanPickup = true;
		}

		camShakeManager.ShakeMagnitude = Settings.DeathShakeMagnitude;
		OneShotAudioManager.PlayOneShotAudio(Settings.PlayerDeathSounds, transform.position);
		gameManager.StartNextRound();

		gameObject.SetActive(false);
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
		projectile.ownCollider.enabled = enableState;
	}

	private void PullInProjectilesInstant()
	{
		var hits = Physics.SphereCastAll(transform.position, Settings.ProjectileMagnetRadius * .25f, Vector3.forward, 0f, Settings.ProjectileLayer);
		for (int i = 0; i < hits.Length; i++)
		{
			var projectile = hits[i].collider.GetComponent<Projectile>();
			if (projectile.CanPickup)
			{
				PickupProjectile(projectile);
			}
		}
	}

	public IEnumerator DashSequence()
	{
		OneShotAudioManager.PlayOneShotAudio(Settings.PlayerDashSounds, transform.position);

		CharController.detectCollisions = false;

		var main = afterImageParticleSystem.main;
		main.duration = Settings.DashDuration + Settings.DashAfterimagePadding;
		afterImageParticleSystem.Play();

		float baseSpeed = currentMovespeed;
		currentMovespeed = Settings.DashSpeed;

		yield return new WaitForSeconds(Settings.DashDuration);
		currentMovespeed = baseSpeed;
		CharController.detectCollisions = true;
	}

	public void CreatePortal()
	{
		if (!Physics.Raycast
			(transform.position + new Vector3(0f, projectileLaunchPos.position.y, 0f), 
			transform.forward, 
			out var hit, 1000f, 
			Settings.TeleportCompatibleLayers)) return;

		if (portalOne == null) portalOne = GameManager.Instance.SpawnObject(Settings.PortalPrefab.gameObject).GetComponent<Portal>();
		if (portalTwo == null) portalTwo = GameManager.Instance.SpawnObject(Settings.PortalPrefab.gameObject).GetComponent<Portal>();


		var yRot = Vector3.Angle(Vector3.right, hit.normal);
		var rotation = Quaternion.Euler(0f, yRot, 90f);
		portalOne.transform.position = hit.point;
		portalOne.transform.rotation = rotation;
		
		if (hit.collider.CompareTag(Settings.WallTag))
		{
			Vector3 finalPos;
			while (true)
			{
				var bounds = hit.collider.bounds;
				Vector3 inDir = hit.transform.position - hit.point;

				float xPerc = inDir.x / (bounds.size.x * .5f);
				float zPerc = inDir.z / (bounds.size.z * .5f);
				if (Mathf.Abs(xPerc) > Mathf.Abs(zPerc))
				{
					inDir.z *= -1f;
				}
				else
				{
					inDir.x *= -1f;
				}

				finalPos = hit.transform.position + inDir;

				if (Physics.Raycast
					(finalPos + (hit.point - finalPos) * .01f,
					finalPos - hit.point,
					out var secondHit, .1f,
					Settings.TeleportCompatibleLayers))
				{
					hit = secondHit;
				}
				else break;
			}

			portalTwo.transform.position = finalPos;
			portalTwo.transform.rotation = rotation;
		}
		else if(hit.collider.CompareTag(Settings.OuterWallTag))
		{
			var wallFound = Physics.Raycast(hit.point, hit.normal, out var hitOuter, 100f, Settings.OuterWallLayer);
			portalTwo.transform.position = hitOuter.point;
			portalTwo.transform.rotation = rotation;
		}

		portalOne.ConnectPortals(portalTwo);
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
		if (activePowerUps.ContainsKey(type)) activePowerUps[type] += Settings.PowerUpDuration;
		else
		{
			activePowerUps.Add(type, Settings.PowerUpDuration);
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
			var powerUp = hit.gameObject.GetComponent<PowerUp>();
			OnPowerUpCollect(powerUp.type);
			hit.gameObject.SetActive(false);
		}
	}

	public void Reset()
	{
		// clear current loaded projectiles
		loadedProjectiles.Clear();

		// disable active power ups and shot inputs
		activePowerUps.Clear();
		shooting = false;

		// spawn inital shots
		var instance = GameManager.Instance;
		for (int i = 0; i < Settings.startProjectileCount; i++)
		{
			PickupProjectile(instance.SpawnObject(projectilePrefab).GetComponent<Projectile>());
		}

		// reset cooldowns
		currentShotCooldown = 0f;
		currentDashCooldown = 0f;
		currentParryCooldown = 0f;
		currentProjectileReloadCooldown = 0f;


		// set active in case player died
		gameObject.SetActive(true);

		// teleport char controller to spawn pos
		CharController.enabled = false;
		transform.position = startPos;
		CharController.enabled = true;
	}
}
