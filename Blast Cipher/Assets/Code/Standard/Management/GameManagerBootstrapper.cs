using EZCameraShake;
using Networking;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GameManagerBootstrapper : Bootstrapper
{
	public MonoServer Server;
	public MonoClient Client;
	public PostProcessVolume PostProcessing;
	public InputMaster InputMaster;
	public float ShakeMagnitude;
	public float ShakeRoughness;

    public CScriptableMap[] StandardMaps = new CScriptableMap[0];
    public CScriptableCharacter[] StandardCharacters = new CScriptableCharacter[0];
    public Material[] CharacterMaterials = new Material[0];

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
