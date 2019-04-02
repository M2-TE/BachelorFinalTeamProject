using UnityEngine;

public class GameManagerBootstrapper : MonoBehaviour
{
	public InputMaster InputMaster;
	public Camera MainCam;

	private void OnEnable()
	{
		GameManager.Instance.RegisterBootstrapper(this);
	}
	private void OnDisable() => GameManager.Instance.UnregisterBootstrapper();

	private void Awake()
	{
		Cursor.lockState = CursorLockMode.Confined;
		InputMaster.Enable();
	}

	private void Start()
	{
		
	}
}
