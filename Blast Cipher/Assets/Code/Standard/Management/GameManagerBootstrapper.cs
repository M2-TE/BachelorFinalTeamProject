using EZCameraShake;
using UnityEngine;

public class GameManagerBootstrapper : Bootstrapper
{
	public InputMaster InputMaster;
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

	private void Update() => gameManager.TriggerExtendedUpdates();
}
