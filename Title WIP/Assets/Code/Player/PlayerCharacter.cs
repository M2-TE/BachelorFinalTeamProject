using System;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
	[SerializeField] private int controlID;
	[SerializeField] private float movespeedMod;

	private GameManager gameManager;
	private CharacterController charController;

	private const string horizontalAxis = "Horizontal";
	private const string verticalAxis = "Vertical";

	private void Awake()
	{
		gameManager = GameManager.Instance;
		charController = GetComponent<CharacterController>();
	}

	private void Update()
	{
		UpdateMovement();
		UpdateLookRotation();
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
		var objectPos = gameManager.MainCam.WorldToScreenPoint(transform.position, Camera.MonoOrStereoscopicEye.Mono);
		objectPos.z = 0f;
		var lookDir =  Input.mousePosition - objectPos;
		transform.rotation = Quaternion.LookRotation(new Vector3(lookDir.x, 0f, lookDir.y), transform.up);
	}
}
