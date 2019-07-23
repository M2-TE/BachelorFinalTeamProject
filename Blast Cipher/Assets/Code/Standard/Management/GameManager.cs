using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public sealed class GameManager
{
	#region Singleton Implementation
	private GameManager()
    {
        ContentHolder = CScriptableHolder.Instance;
        LoadStreamingAssets();
    }
	private static GameManager instance;
	public static GameManager Instance { get => instance ?? (instance = new GameManager()); }
	#endregion

	#region Properties
	private GameManagerBootstrapper _bootstrapper;
	private GameManagerBootstrapper bootstrapper;

    public CScriptableHolder ContentHolder;
	public readonly InputDevice[] inputDevices = new InputDevice[2];
	public bool playerInputsActive = true;
    public int maxRounds;

	private int roundCount;
	private bool nextRoundStarterInProgress = false;
	private Scene asyncEssentials;
	private Scene currentMainScene;

    private Mesh playerOneMesh, playerTwoMesh, playerThreeMesh, playerFourMesh;
    private int playerOneColor, playerTwoColor, playerThreeColor, playerFourColor;
	#endregion

	public delegate void ExtendedUpdate();

	public readonly List<GameObject> temporaryObjects = new List<GameObject>();
	public readonly List<ExtendedUpdate> extendedUpdates = new List<ExtendedUpdate>();
	private readonly List<PlayerCharacter> registeredPlayerCharacters = new List<PlayerCharacter>(2);

    public Material[] CharacterMaterials => bootstrapper.CharacterMaterials;
    private CScriptableCharacter[] StandardCharacters => bootstrapper.StandardCharacters;
    private CScriptableMap[] StandardMaps => bootstrapper.StandardMaps;

	#region Mono Registrations
	internal void RegisterBootstrapper(GameManagerBootstrapper bootstrapper)
	{
		this.bootstrapper = bootstrapper;

		bootstrapper.PostProcessing.profile.TryGetSettings<Vignette>(out var vignette);
		Effects.Init(bootstrapper.PostProcessing);
	}
	internal void UnregisterBootstrapper() => bootstrapper = null;

	public int RegisterPlayerCharacter(PlayerCharacter playerCharacter)
	{
		registeredPlayerCharacters.Add(playerCharacter);
		return registeredPlayerCharacters.Count - 1;
	}
	public bool UnregisterPlayerCharacter(PlayerCharacter playerCharacter)
	{
		return registeredPlayerCharacters.Remove(playerCharacter);
	}
	#endregion

	#region Scene Loader
	public void LoadScene(string sceneName) => bootstrapper.StartCoroutine(LoadSceneCo(SceneManager.GetSceneByName(sceneName).buildIndex));
	public void LoadScene(int buildIndex) => bootstrapper.StartCoroutine(LoadSceneCo(buildIndex));
	private IEnumerator LoadSceneCo(int buildIndex)
	{
		// unload all unwanted scenes
		Scene scene;
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			scene = SceneManager.GetSceneAt(i);

			if (scene.buildIndex == asyncEssentials.buildIndex)
			{
				// clean up leftover projectiles that, for some fucking reason, end up in this scene sometimes
				var objects = scene.GetRootGameObjects();
				for (int k = 0; k < objects.Length; k++)
				{
					if (objects[k].CompareTag("Projectile")) MonoBehaviour.Destroy(objects[k]);
				}
				continue;
			}
			else SceneManager.UnloadSceneAsync(scene.buildIndex, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
		}

		// load new scene
		var operation = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);

		// wait until new scene is fully loaded
		while (!operation.isDone) yield return null;

		// set new scene active after single frame delay
		SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(buildIndex));
	}
	#endregion

    internal void LoadStreamingAssets()
    {
        string path = Application.streamingAssetsPath + "/ContentHolder.json";
        if (File.Exists(path))
        {
            string jsonString = File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(jsonString,ContentHolder);
            for (int i = 0; i < ContentHolder.CharacterPaths.Count; i++)
            {
                jsonString = File.ReadAllText(Application.streamingAssetsPath + ContentHolder.CharacterPaths[i]);
                CScriptableCharacter character = ScriptableObject.CreateInstance<CScriptableCharacter>();
                JsonUtility.FromJsonOverwrite(jsonString, character);
                ContentHolder.Characters.Add(character);
                Debug.Log("Load: " + ContentHolder.Characters[i].CharacterID);
            }
            for (int i = 0; i < ContentHolder.MapPaths.Count; i++)
            {
                jsonString = File.ReadAllText(Application.streamingAssetsPath + ContentHolder.MapPaths[i]);
                CScriptableMap map = ScriptableObject.CreateInstance<CScriptableMap>();
                JsonUtility.FromJsonOverwrite(jsonString, map);
                ContentHolder.Maps.Add(map);
                Debug.Log("Load: " + ContentHolder.Maps[i].MapID);
            }
        }
        else
        {
            SaveStreamingAssets();
        }
    }

    internal void SaveStreamingAssets()
    {
        string aboutToBeJsonString = JsonUtility.ToJson(ContentHolder);
        File.WriteAllText(Application.streamingAssetsPath + "/ContentHolder.json", aboutToBeJsonString);
        for (int i = 0; i < ContentHolder.CharacterPaths.Count; i++)
        {
            aboutToBeJsonString = JsonUtility.ToJson(ContentHolder.Characters[i]);
            File.WriteAllText(Application.streamingAssetsPath + ContentHolder.CharacterPaths[i], aboutToBeJsonString);
            Debug.Log("Save: " + ContentHolder.Characters[i].CharacterID);
        }
        for (int i = 0; i < ContentHolder.MapPaths.Count; i++)
        {
            aboutToBeJsonString = JsonUtility.ToJson(ContentHolder.Maps[i]);
            File.WriteAllText(Application.streamingAssetsPath + ContentHolder.MapPaths[i], aboutToBeJsonString);
            Debug.Log("Save: " + ContentHolder.Maps[i].MapID);
        }
    }

    internal void CheckForStandardContent()
    {
        if(ContentHolder.Characters.Count <= 0)
        foreach (var character in StandardCharacters)
        {
            ContentHolder.AddCharacter(character);
        }
        if(ContentHolder.Maps.Count <= 0)
        foreach (var map in StandardMaps)
        {
            ContentHolder.AddMap(map);
        }
        SaveStreamingAssets();
    }

	internal void TriggerExtendedUpdates()
	{
		for (int i = 0; i < extendedUpdates.Count; i++) extendedUpdates[i]();
	}

	internal void SetAsyncEssentialsScene(Scene scene)
	{
		asyncEssentials = scene;
	}

	public PlayerCharacter RequestNearestPlayer(PlayerCharacter requestSender)
	{
		if (registeredPlayerCharacters.Count < 2) return null;
		return registeredPlayerCharacters[registeredPlayerCharacters.IndexOf(requestSender) == 0 ? 1 : 0];
	}

	public GameObject SpawnObject (GameObject prefab)
	{
		var go = GameObject.Instantiate(prefab);
		temporaryObjects.Add(go);
		return go;
	}

	//public void StartNextRound()
	//{
	//	if (!nextRoundStarterInProgress)
	//	{
	//		nextRoundStarterInProgress = true;
	//		//playerInputsActive = false;
	//		if(roundCount != 0 && roundCount % 3 == 0)
	//		{
	//			MusicManager.Instance.TransitionToNextIntensity(OnNextMusicBar);
	//		}
	//		else
	//		{
	//			MusicManager.Instance.RoundTransitionSmoother(OnNextMusicBar);
	//		}
	//		bootstrapper.StartCoroutine(TimeScalerOnRoundTransition());

	//		roundCount++;
	//	}
	//}

	public void StartNextRound()
	{
		if (!nextRoundStarterInProgress)
		{
            //if(roundCount >= maxRounds-1)
            //{
            //    BackToMenu();
            //    return;
            //}
			nextRoundStarterInProgress = true;
			if (/*roundCount != 0 &&*/ roundCount % 2 == 0)
			{
				MusicManager.Instance.RoundTransitionSmoother(OnNextMusicBar, true);
			}
			else
			{
				MusicManager.Instance.RoundTransitionSmoother(OnNextMusicBar, false);
			}
			bootstrapper.StartCoroutine(TimeScalerOnRoundTransition());

			roundCount++;
		}
	}

    public void AssignPlayerMeshes(Mesh playerOne, Mesh playerTwo, Mesh playerThree, Mesh playerFour)
    {
        playerOneMesh = playerOne;
        playerTwoMesh = playerTwo;
        playerThreeMesh = playerThree;
        playerFourMesh = playerFour;
    }

    public void AssignPlayerColors(int colorOne, int colorTwo, int colorThree, int colorFour)
    {
        this.playerOneColor = colorOne;
        this.playerTwoColor = colorTwo;
        this.playerThreeColor = colorThree;
        this.playerFourColor = colorFour;
    }

    public Mesh GetMeshByPlayerID(int id)
    {
        Mesh m = null;
        switch (id)
        {
            case 0:
                m = playerOneMesh;
                break;
            case 1:
                m = playerTwoMesh;
                break;
            case 2:
                m = playerThreeMesh;
                break;
            case 3:
                m = playerFourMesh;
                break;
            default:
                break;
        }
        if (m == null)
            m = MeshGenerator.GenerateMeshFromScriptableObject(ContentHolder.Characters[0]);
        return m;
    }

    public Material GetMaterialByPlayerID(int id)
    {
        Material m = null;
        switch (id)
        {
            case 0:
                m = CharacterMaterials[playerOneColor];
                break;
            case 1:
                m = CharacterMaterials[playerTwoColor];
                break;
            case 2:
                m = CharacterMaterials[playerThreeColor];
                break;
            case 3:
                m = CharacterMaterials[playerFourColor];
                break;
            default:
                break;
        }
        if (m == null)
            m = CharacterMaterials[0];
        return m;
    }

    private void BackToMenu()
    {
        LoadScene(4);
        roundCount = 0;
        maxRounds = 0;
        inputDevices[0] = null;
        inputDevices[1] = null;
    }

	private IEnumerator TimeScalerOnRoundTransition()
	{
		float scaleModPerFrame = 1f;
		float fixedDeltaTime = Time.fixedDeltaTime;
		while (nextRoundStarterInProgress)
		{
			Time.timeScale -= scaleModPerFrame * Time.deltaTime;
			Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale;
			yield return new WaitForEndOfFrame();
		}
		Time.fixedDeltaTime = fixedDeltaTime;
		Time.timeScale = 1f;
	}

	private void OnNextMusicBar()
	{
		//LoadScene("Gameplay Proto");
		ResetLevel();

		playerInputsActive = true;
		nextRoundStarterInProgress = false;
	}

	private void ResetLevel()
	{
		for (int i = 0; i < temporaryObjects.Count; i++)
		{
			GameObject.Destroy(temporaryObjects[i]);
		}
		for (int i = 0; i < registeredPlayerCharacters.Count; i++)
		{
			registeredPlayerCharacters[i].Reset();
		}
	}
}