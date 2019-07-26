using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GameManagerBootstrapper : Bootstrapper
{
	public MusicDictionary musicDict;
	public AudioListener EmergencyListener;
	public PostProcessVolume PostProcessing;
	public InputMaster InputMaster;
	public float ShakeMagnitude;
	public float ShakeRoughness;

    public bool AllowOneControllerGameStart = true;

    public CScriptableMap[] StandardMaps = new CScriptableMap[0];
    public CScriptableCharacter[] StandardCharacters = new CScriptableCharacter[0];
    public Material[] CharacterMaterials = new Material[0], TeamMaterials = new Material[4];


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
		EmergencyListener.enabled = false;
	}

	private void Start()
	{
		var musicPlayer = MusicManager.Instance;
		musicPlayer.PlayMusic(musicDict.MusicDict[0].Audio);
	}

	private void Update() => gameManager.TriggerExtendedUpdates();
}
