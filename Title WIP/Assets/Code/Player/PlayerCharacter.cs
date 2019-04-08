using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;
using Random = UnityEngine.Random;

public abstract class InputSystemMonoBehaviour : MonoBehaviour
{
	private void OnEnable() => RegisterActions();
	private void OnDisable() => UnregisterActions();

	protected abstract void RegisterActions();
	protected abstract void UnregisterActions();
}

public class PlayerCharacter : InputSystemMonoBehaviour
{
	#region Fields
	[SerializeField] private InputMaster inputMaster;
	[SerializeField] private Animator parryAnimator;
	[SerializeField] private Transform projectileOrbitCenter;
	[SerializeField] private Transform projectileLaunchPos;
	[SerializeField] private GameObject projectilePrefab;
	[SerializeField] private int controlDeviceIndex;

	[Header("Combat")]
	[SerializeField] private float movespeedMod;
	[SerializeField] private float gravityMod; 
	[SerializeField] private float jumpForce; 

	[SerializeField] private float shotCooldown;
	[SerializeField] private float shotPrepSpeed;
	[SerializeField] private float chargeupIterationSpeedMod;
	[SerializeField] private float shotStrength;

	[Header("Projectile Orbit")]
	[SerializeField] private Vector3 projectileOrbitDelta;
	[SerializeField] private float projectileMaxOrbitDistPerFrame;
	[SerializeField] private float projectileMovementOrbitDist;
	[SerializeField] private float projectileMovementInterpolation;
	[SerializeField] private float projectileOrbitDist;
	[SerializeField] private float projectileOrbitHeightMod;
	[SerializeField] private float projectileRotationMin;
	[SerializeField] private float projectileRotationMax;

	private GameManager gameManager;
	private CharacterController charController;
	private InputMaster input;

	private readonly List<Projectile> loadedProjectiles = new List<Projectile>();

	private Vector2 movementInput;
	private Vector2 aimInput;
	private Quaternion currentCoreRotation = Quaternion.identity;
	private Quaternion coreRotationDelta = Quaternion.identity;

	private float currentJumpForce = 0f;
	private float currentShotCooldown = 0f;

	private PlayerCharacter aimLockTarget;
	private bool aimLocked = false;
	#endregion

	protected override void RegisterActions()
	{
		input.Player.Movement.performed += UpdateMovementControlled;
		input.Player.Aim.performed += UpdateLookRotationControlled;
		input.Player.Shoot.performed += TriggerShotControlled;
		input.Player.Jump.performed += TriggerJump;
		input.Player.Parry.performed += TriggerParry;
		input.Player.LockAim.performed += TriggerAimLock;

		input.Player.Debug.performed += TriggerDebugAction;
	}

	protected override void UnregisterActions()
	{
		input.Player.Movement.performed -= UpdateMovementControlled;
		input.Player.Aim.performed -= UpdateLookRotationControlled;
		input.Player.Shoot.performed -= TriggerShotControlled;
		input.Player.Jump.performed -= TriggerJump;
		input.Player.Parry.performed -= TriggerParry;
		input.Player.LockAim.performed -= TriggerAimLock;

		input.Player.Debug.performed -= TriggerDebugAction;
	}

	private void Awake()
	{
		gameManager = GameManager.Instance;
		input = gameManager.InputMaster;
		gameManager.RegisterPlayerCharacter(this);

		charController = GetComponent<CharacterController>();

		// convert core rotation v3 to quaternion
		coreRotationDelta = Quaternion.Euler(projectileOrbitDelta);
	}

	private void Start()
	{
		// since its only 1o1, this only needs to be called once
		aimLockTarget = gameManager.RequestNearestPlayer(this);

		// spawn initial projectile
		PickupProjectile(Instantiate(projectilePrefab).GetComponent<Projectile>());
	}

	private void Update()
	{
		UpdateMovement();
		UpdateLookRotation();

		UpdateProjectileOrbit();

		UpdateMiscValues();
	}

	#region InputSystem event calls
	private void UpdateMovementControlled(InputAction.CallbackContext ctx)
	{
		if (IsMatchingDeviceID(ctx)) movementInput = ctx.ReadValue<Vector2>();
	}

	private void UpdateLookRotationControlled(InputAction.CallbackContext ctx)
	{
		if (IsMatchingDeviceID(ctx)) aimInput = ctx.ReadValue<Vector2>();
	}

	private void TriggerShotControlled(InputAction.CallbackContext ctx)
	{
		if (currentShotCooldown == 0f && loadedProjectiles.Count > 0 && IsMatchingDeviceID(ctx))
		{
			var projectile = loadedProjectiles[0];
			loadedProjectiles.Remove(projectile);
			Shoot(projectile);
		}
	}

	private void TriggerJump(InputAction.CallbackContext ctx)
	{
		if(charController.isGrounded && IsMatchingDeviceID(ctx)) currentJumpForce = jumpForce;
	}

	private void TriggerParry(InputAction.CallbackContext ctx)
	{
		if (IsMatchingDeviceID(ctx)) parryAnimator.SetTrigger("ConstructParryShield");
	}

	private void TriggerAimLock(InputAction.CallbackContext ctx)
	{
		if (IsMatchingDeviceID(ctx)) aimLocked = !aimLocked;
	}

	private void TriggerDebugAction(InputAction.CallbackContext ctx)
	{
		if (IsMatchingDeviceID(ctx))
			PickupProjectile(Instantiate(projectilePrefab).GetComponent<Projectile>());
	}
	#endregion

	private bool IsMatchingDeviceID(InputAction.CallbackContext ctx)
	{
		return gameManager.registeredControlDeviceIDs.Count > controlDeviceIndex
			&& ctx.control.device.id == gameManager.registeredControlDeviceIDs[controlDeviceIndex];
	}

	private void SetProjectileEnabled(Projectile projectile, bool enableState)
	{
		if (enableState) projectile.rgb.WakeUp();
		else projectile.rgb.Sleep();

		projectile.rgb.useGravity = enableState;
		projectile.collider.enabled = enableState;
	}

	private void UpdateMovement()
	{
		Camera mainCam = gameManager.MainCam; // dont save reference to main cam to avoid static MonoBehavior ref

		var camHorizontal = new Vector3(mainCam.transform.right.x, 0f, mainCam.transform.right.z).normalized;
		var camVertical = new Vector3(mainCam.transform.forward.x, 0f, mainCam.transform.forward.z).normalized;

		float mod = Time.deltaTime * movespeedMod;
		var movement = Vector3.ClampMagnitude
			(camHorizontal * movementInput.x 
			+ camVertical * movementInput.y, 1f) * mod; // move player relative to camera

		movement.y = currentJumpForce += Physics.gravity.y * Time.deltaTime * gravityMod; // gravity and jumping
		
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
			else lookDir = (aimLockTarget.transform.position - transform.position).normalized;
		}
		else if (aimInput.sqrMagnitude < .1f) return;
		else lookDir = new Vector3(aimInput.x, 0f, aimInput.y);

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
			Vector3 orbitVec = Vector3.forward * projectileOrbitDist; // identity vector for all projectiles
			orbitVec = selfRotation * orbitVec; // vector from orbit center to target position (local space)
			targetPos = projectileOrbitCenter.position + orbitVec;

			if (movementInput.sqrMagnitude > .5f)
			{
				Vector3 targetPosTwo = transform.position + transform.up * projectileMovementOrbitDist;
				targetPos = Vector3.Lerp(targetPos, targetPosTwo, projectileMovementInterpolation);
			}

			projectile.transform.position = Vector3.MoveTowards
				(projectile.transform.position, 
				targetPos, 
				Vector3.Distance(projectile.transform.position, targetPos) * projectileMaxOrbitDistPerFrame * Time.deltaTime);

			UpdateProjectileRotation(projectile);
		}
	}

	private void UpdateProjectileRotation(Projectile projectile)
	{
		projectile.transform.rotation *= 
			Quaternion.Euler
			(Random.Range(projectileRotationMin, projectileRotationMax) * Time.deltaTime, 
			Random.Range(projectileRotationMin, projectileRotationMax) * Time.deltaTime, 
			Random.Range(projectileRotationMin, projectileRotationMax) * Time.deltaTime);
	}
	
	private void UpdateMiscValues()
	{
		currentShotCooldown = currentShotCooldown > 0f ? currentShotCooldown - Time.deltaTime : 0f;
	}

	private void Shoot(Projectile projectile)
	{
		// move projectile to launch position
		//SetProjectileEnabled(projectile, false);
		//for(int iteration = 0; projectile != null && Vector3.Distance(projectile.transform.position, projectileLaunchPos.position) > .2f; iteration++)
		//{
		//	projectile.transform.position = Vector3.MoveTowards(projectile.transform.position,
		//		projectileLaunchPos.position,
		//		(shotPrepSpeed + iteration * chargeupIterationSpeedMod * Time.deltaTime) * Time.deltaTime);

		//	yield return null;
		//}

		projectile.transform.position = projectileLaunchPos.transform.position;
		SetProjectileEnabled(projectile, true);

		// launch projectile in aim direction
		var forceVec = projectileLaunchPos.position - transform.position;
		forceVec.y = 0f;
		projectile.rgb.AddForce(forceVec.normalized * shotStrength, ForceMode.Impulse);

		projectile = null;
	}

	public void TriggerDeath()
	{
		Debug.Log(name + " died");
		for (int i = 0; i < loadedProjectiles.Count; i++)
		{
			SetProjectileEnabled(loadedProjectiles[i], true);
			loadedProjectiles[i].rgb.constraints = RigidbodyConstraints.None;
			loadedProjectiles[i].canPickup = true;
		}
		Destroy(gameObject);
	}

	public void PickupProjectile(Projectile projectile)
	{
		loadedProjectiles.Add(projectile);
		SetProjectileEnabled(projectile, false);
		projectile.canPickup = false;
	}

	private void OnControllerColliderHit(ControllerColliderHit hit) // pickup logic
	{
		if(hit.collider.CompareTag("Projectile"))
		{
			var projectile = hit.collider.GetComponent<Projectile>();
			if (projectile.canPickup) PickupProjectile(projectile);
		}
	}
}
