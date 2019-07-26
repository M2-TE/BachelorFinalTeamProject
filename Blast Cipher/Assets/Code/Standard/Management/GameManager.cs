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
	public readonly InputDevice[] inputDevices = new InputDevice[4];
	public bool playerInputsActive = true;
    public int maxRounds;

	private int roundCount = 1;
	private bool nextRoundStarterInProgress = false;
	private Scene asyncEssentials;
	private Scene currentMainScene;

    public float MenuSoundsVolume = .2f;

    private Mesh[] playerMeshes = new Mesh[4] { null, null, null, null };
    private int[] playerColors = new int[4] { 0, 0, 0, 0};
    private int[] playerTeams = new int[4] { 0, 1, 2, 3};
#endregion

public delegate void ExtendedUpdate();

	public readonly List<GameObject> temporaryObjects = new List<GameObject>();
	public readonly List<ExtendedUpdate> extendedUpdates = new List<ExtendedUpdate>();
	private readonly List<PlayerCharacter> registeredPlayerCharacters = new List<PlayerCharacter>(2);

    public bool AllowOneControllerGameStart => bootstrapper.AllowOneControllerGameStart;

    public Material[] CharacterMaterials => bootstrapper.CharacterMaterials;
    public Material[] TeamMaterials => bootstrapper.TeamMaterials;
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
	//public void LoadScene(string sceneName) => bootstrapper.StartCoroutine(LoadSceneCo(SceneManager.GetSceneByName(sceneName).buildIndex));
	public void LoadScene(int buildIndex) => bootstrapper.StartCoroutine(LoadSceneCo(buildIndex));
	private IEnumerator LoadSceneCo(int buildIndex)
	{
		// remove user control from currently unloading scene
		SceneManager.SetActiveScene(asyncEssentials);

		var token = new LoadingScreenHandler.LoadingScreenProgressToken();
		LoadingScreenHandler.ShowLoadingScreen(token);

		while(!token.ScreenFullyShown) { yield return null; }
		
		// unload all unwanted scenes
		Scene scene;
		List<int> scenesToUnload = new List<int>();
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			scene = SceneManager.GetSceneAt(i);

			if (scene.buildIndex == asyncEssentials.buildIndex) continue;
			else scenesToUnload.Add(scene.buildIndex);
		}

		// unload old scenes
		for (int i = 0; i < scenesToUnload.Count; i++)
		{
			SceneManager.UnloadSceneAsync(scenesToUnload[i], UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
		}
		bootstrapper.EmergencyListener.enabled = true;

		// load new scene
		var loadOperation = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
		loadOperation.allowSceneActivation = false;

		while (!token.TransitionComplete) { yield return null; } // wait until loading is starting the transition into new scene
		loadOperation.allowSceneActivation = true; // now allow to complete loading of level

		MusicManager.Instance.PlayMusic(bootstrapper.musicDict.MusicDict[buildIndex].Audio);

		// wait until new scene is fully loaded
		while (!loadOperation.isDone) { yield return null; }
		bootstrapper.EmergencyListener.enabled = false;


		// set new scene active
		SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(buildIndex));

		// another smol hacc to refresh the post processing volume
		bootstrapper.PostProcessing.gameObject.SetActive(false);
		bootstrapper.PostProcessing.gameObject.SetActive(true);
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

	public void StartNextRound()
	{
		if (!nextRoundStarterInProgress)
		{
			//if (roundCount >= maxRounds - 1)
			//{
			//	BackToMenu();
			//	return;
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

    public void AssignPlayerMeshes(Mesh[] playerMeshes)
    {
        this.playerMeshes = playerMeshes;
    }

    public void AssignPlayerColors(int[] playerColors)
    {
        this.playerColors = playerColors;
    }

    public void AssignPlayerTeams(int[] playerTeams)
    {
        this.playerTeams = playerTeams;
    }

    public Mesh GetMeshByPlayerID(int id)
    {
        Mesh m = playerMeshes[id];
        if (m == null)
            m = MeshGenerator.GenerateMeshFromScriptableObject(ContentHolder.Characters[0]);
        return m;
    }

    public Material GetMaterialByPlayerID(int id)
    {
        Material m = CharacterMaterials[playerColors[id]];
        
        if (m == null)
            m = CharacterMaterials[0];
        return m;
    }

    public int GetTeamByPlayerID(int id)
    {
        return playerTeams[id];
    }

    private void BackToMenu()
    {
        LoadScene(0);
        roundCount = 0;
        maxRounds = 0;
        for (int i = 0; i < inputDevices.Length; i++)
        {
            inputDevices[i] = null;
        }
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