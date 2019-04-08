using EZCameraShake;
using UnityEngine;

public class GameManagerBootstrapper : MonoBehaviour
{
	public InputMaster InputMaster;
	public Camera MainCam;
	public float ShakeMagnitude;
	public float ShakeRoughness;

	[SerializeField] private float shakeMagnitudeDecline;
	private GameManager gameManager;

	private void OnEnable() => GameManager.Instance.RegisterBootstrapper(this);
	private void OnDisable() => GameManager.Instance.UnregisterBootstrapper();

	private void Awake()
	{
		//Cursor.lockState = CursorLockMode.Locked;
		//Cursor.visible = false;
		InputMaster.Enable();
		gameManager = GameManager.Instance;
	}

	private void Start()
	{
		gameManager.ShakeMagnitude = ShakeMagnitude;
		gameManager.ShakeRoughness = ShakeRoughness;
		gameManager.ShakeMagnitudeDecline = shakeMagnitudeDecline;
	}

	private void Update() => gameManager.ExtendedUpdate();
}
