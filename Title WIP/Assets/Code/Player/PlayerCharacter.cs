using System;
using UnityEngine;
using UnityEngine.Experimental.Input;

public abstract class InputSystemMonoBehaviour : MonoBehaviour
{
	private void OnEnable() => RegisterActions();
	private void OnDisable() => UnregisterActions();

	protected abstract void RegisterActions();
	protected abstract void UnregisterActions();
}

public class PlayerCharacter : InputSystemMonoBehaviour
{
	private enum ControlType { Mouse, Controller }

	[SerializeField] private InputMaster inputMaster;
	[SerializeField] private int controlID;
	[SerializeField] private ControlType controlType;
	[Space]
	[SerializeField] private float movespeedMod;

	private GameManager gameManager;
	private CharacterController charController;
	private InputMaster input;

	private const string horizontalAxis = "Horizontal";
	private const string verticalAxis = "Vertical";
	
	protected override void RegisterActions()
	{
		input.Player.Movement.performed += DoThing;
		Debug.Log("noted");
	}

	protected override void UnregisterActions()
	{
		input.Player.Movement.performed -= DoThing;
	}

	private void Awake()
	{
		gameManager = GameManager.Instance;
		input = gameManager.InputMaster;
		charController = GetComponent<CharacterController>();

	}

	private void DoThing(InputAction.CallbackContext ctx)
	{

	}

	private void Update()
	{
		//UpdateMovement();
		//UpdateLookRotation();
	}

	private void UpdateMovement()
	{
		Camera mainCam = gameManager.MainCam; // dont save reference to main cam to avoid static MonoBehavior ref

		var hInput = new Vector3(mainCam.transform.right.x, 0f, mainCam.transform.right.z).normalized;
		var vInput = new Vector3(mainCam.transform.forward.x, 0f, mainCam.transform.forward.z).normalized;

		float mod = Time.deltaTime * movespeedMod;
		var movement = Vector3.ClampMagnitude(hInput * Input.GetAxis(horizontalAxis) 
			+ vInput * Input.GetAxis(verticalAxis), 1f) * mod; // move player relative to camera
		if (!charController.isGrounded) movement.y = Physics.gravity.y * Time.deltaTime; // apply gravity if not grounded
		
		charController.Move(movement);
	}

	private void UpdateLookRotation()
	{
		Vector3 lookDir = default;
		switch (controlType)
		{
			default:
			case ControlType.Mouse:
				var objectPos = gameManager.MainCam.WorldToScreenPoint(transform.position, Camera.MonoOrStereoscopicEye.Mono);
				objectPos.z = 0f;
				lookDir = Input.mousePosition - objectPos;
				break;

			case ControlType.Controller:
				//Input.GetAxis()
				break;
		}

		transform.rotation = Quaternion.LookRotation(new Vector3(lookDir.x, 0f, lookDir.y), transform.up);
	}
}
