using UnityEngine;
using UnityEngine.Experimental.Input;

public class ControllerAssignment : MonoBehaviour
{
	[SerializeField] private InputMaster input;
	private InputDevice currentAssigningDevice;

	private void Awake()
	{
		gameObject.SetActive(false);
		input.General.RegisterDevice.performed += DisplayAssignmentWindow;
	}

	private void Update()
	{
		if (currentAssigningDevice != null && Input.GetKeyDown(KeyCode.Escape)) SetActive(false);
	}

	private void DisplayAssignmentWindow(InputAction.CallbackContext ctx)
	{
		if (currentAssigningDevice == null)
		{
			currentAssigningDevice = ctx.control.device;
			SetActive(true);
		}
	}

	private void OnDPadInput(InputAction.CallbackContext ctx)
	{
		if (ctx.control.device != currentAssigningDevice) return; // only accept input from chosen device

		var manager = GameManager.Instance;
		float xInput = ctx.ReadValue<Vector2>().x;
		if (xInput > 0f)
		{
			manager.inputDevices[0] = currentAssigningDevice;
			SetActive(false);
		}
		else if(xInput < 0f)
		{
			manager.inputDevices[1] = currentAssigningDevice;
			SetActive(false);
		}
	}

	private void SetActive(bool activeState)
	{
		gameObject.SetActive(activeState);

		if (activeState)
		{
			input.General.DPadInput.performed += OnDPadInput;
			Time.timeScale = 0f;
			Debug.Log("triggered");
		}
		else
		{
			input.General.DPadInput.performed -= OnDPadInput;
			currentAssigningDevice = null;
			Time.timeScale = 1f;
		}


	}
}
