using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public delegate void CaseDelegate();
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
    public MatchSettings matchSettings = new MatchSettings
		(15, 0, true, 
		new bool[3] { true, true, true }, 
		new int[3] { 9, 9, 9 }, 
		new int[3] { 3, 3, 3 }, 
		6, 4, 2);

	private int cachedIndex = 0;
	private int roundCount = 1;
	private int currentPhase = 0;
	private bool nextRoundStarterInProgress = false;
	private Scene asyncEssentials;
	private Scene currentMainScene;

	public bool gameInProgress = false;
	public CaseDelegate OnLevelChange;
	public float MenuSoundsVolume = .2f;

    public List<Mesh> WinnerMeshes = new List<Mesh>();

    private Mesh[] playerMeshes = new Mesh[4] { null, null, null, null };
    private bool[] playersAlive = new bool[4] { false, false, false, false };
    private int[] playerColors = new int[4] { 0, 0, 0, 0};
    private int[] playerTeams = new int[4] { 0, 1, 2, 3};
    private int[] teamPoints = new int[4] { 0, 0, 0, 0 };
	#endregion

	public delegate void ExtendedUpdate();

	public readonly List<GameObject> temporaryObjects = new List<GameObject>();
	public readonly List<ExtendedUpdate> extendedUpdates = new List<ExtendedUpdate>();
	private readonly List<PlayerCharacter> registeredPlayerCharacters = new List<PlayerCharacter>(4);

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
		UpdateGlobalColorScheme();
		gameInProgress = true;
		CamMover.Instance.Players.Add(playerCharacter);
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
		for (int i = 0; i < registeredPlayerCharacters.Count; i++)
		{
			//GameObject.Destroy(registeredPlayerCharacters[i].gameObject);
		}
		registeredPlayerCharacters.Clear();

		// remove user control from currently unloading scene
		SceneManager.SetActiveScene(asyncEssentials);

		var token = new LoadingScreenHandler.LoadingScreenProgressToken();
		LoadingScreenHandler.ShowLoadingScreen(token);

		while(!token.ScreenFullyShown) { yield return null; }

		OnLevelChange();

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
																 // set new scene active
		loadOperation.allowSceneActivation = true; // now allow to complete loading of level
		loadOperation.completed += OnLoadDone;

		cachedIndex = buildIndex;
	}

	private void OnLoadDone(AsyncOperation obj)
	{
		bootstrapper.EmergencyListener.enabled = false;
		SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(cachedIndex));

		// another smol hacc to refresh the post processing volume
		bootstrapper.PostProcessing.gameObject.SetActive(false);
		bootstrapper.PostProcessing.gameObject.SetActive(true);
		Time.timeScale = 1f;

		if (bootstrapper.musicDict.MusicDict.ContainsKey(cachedIndex))
		{
			MusicManager.Instance.PlayMusic(bootstrapper.musicDict.MusicDict[cachedIndex].Audio);
		}
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

		float minDist = float.MaxValue;
		PlayerCharacter minChar = null;
		for(int i = 0; i < registeredPlayerCharacters.Count; i++)
		{
			if (registeredPlayerCharacters[i] == requestSender) continue;

			float dist = Vector3.Distance(requestSender.transform.position, registeredPlayerCharacters[i].transform.position);
			if (dist < minDist) minChar = registeredPlayerCharacters[i];
		}

		return minChar;

		//return registeredPlayerCharacters[registeredPlayerCharacters.IndexOf(requestSender) == 0 ? 1 : 0];
	}

	public GameObject SpawnObject (GameObject prefab)
	{
		var go = GameObject.Instantiate(prefab);
		temporaryObjects.Add(go);
		return go;
	}

	private void UpdateGlobalColorScheme()
	{
		float thirdRounds = (float)roundCount / (float)matchSettings.Rounds;

		var cols = bootstrapper.GlobalMatRefs;
		switch (currentPhase)
		{
			default:
			case 0:
				cols.GlobalMat.SetColor("_EmissionColor", cols.blue);
				cols.GlobalDimMat.SetColor("_EmissionColor", cols.dimBlue);
				break;

			case 1:
				cols.GlobalMat.SetColor("_EmissionColor", cols.green);
				cols.GlobalDimMat.SetColor("_EmissionColor", cols.dimGreen);
				break;

			case 2:
				cols.GlobalMat.SetColor("_EmissionColor", cols.red);
				cols.GlobalDimMat.SetColor("_EmissionColor", cols.dimRed);
				break;
		}
	}

	public void StartNextRound()
	{
		if (!nextRoundStarterInProgress)
		{
			UpdateGlobalColorScheme();

			nextRoundStarterInProgress = true;
			
			if ((float)roundCount / (float)matchSettings.Rounds >= 3f / (float)matchSettings.Rounds * ((float)currentPhase + 1f))
			{
				Debug.Log("switch buffered");
				currentPhase++;
				MusicManager.Instance.RoundTransitionSmoother(OnNextMusicBar, true);
			}
			else
			{
				MusicManager.Instance.RoundTransitionSmoother(OnNextMusicBar, false);
			}

			IngameUIManager.Instance.UpdateUI(teamPoints, roundCount);


			bootstrapper.StartCoroutine(TimeScalerOnRoundTransition());
            ResetAlivePlayers();
			for(int i = 0; i < registeredPlayerCharacters.Count; i++)
			{
				registeredPlayerCharacters[i].portalPlaced = false;
			}
		}
	}

    public void SetSettings(MatchSettings settings)
    {
        this.matchSettings = settings;
        teamPoints = new int[4] { 0, 0, 0, 0 };
        ResetAlivePlayers();
    }

    public void PlayerDown(int playerID)
    {
        playersAlive[playerID] = false;

        if (!MoreThenOneTeamLeft())
        {
            int winnerPos = 0;
            for (int i = 0; i < 4; i++)
            {
                if (playersAlive[i])
                    winnerPos = playerTeams[i];
            }
            teamPoints[winnerPos]++;
            StartNextRound();
        }
    }

    private bool MoreThenOneTeamLeft()
    {
        int playerAlive = 0;
        List<int> teamsList = new List<int>();
        for (int i = 0; i < playersAlive.Length; i++)
        {
            if (playersAlive[i] && !teamsList.Contains(playerTeams[i]))
            {
                teamsList.Add(playerTeams[i]);
                playerAlive++;
            }
        }
        return (playerAlive > 1);
    }

    private void ResetAlivePlayers()
    {
        for (int i = 0; i < 4; i++)
        {
            playersAlive[i] = playerMeshes[i] != null;
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

    public bool[] GetActiveTeams()
    {
        bool[] activeTeams = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            bool temp = false;
            for (int j = 0; j < 4; j++)
            {
                if (playerTeams[j].Equals(i) && playerMeshes[j] != null)
                    temp = true;
            }
            activeTeams[i] = temp;
        }
        return activeTeams;
    }

    private void BackToMenu()
    {
        AddWinners();
        LoadScene(5);
        roundCount = 0;
		currentPhase = 0;
		gameInProgress = false;

		for (int i = 0; i < inputDevices.Length; i++)
        {
            inputDevices[i] = null;
        }
    }

    private void AddWinners()
    {
        int winnerTeam = 0;
        int pointholder = 0;
        for (int i = 0; i < teamPoints.Length; i++)
        {
            if (pointholder < teamPoints[i])
            {
                pointholder = teamPoints[i];
                winnerTeam = i;
            }
        }
        for (int i = 0; i < playerMeshes.Length; i++)
        {
            if (playerTeams[i] == winnerTeam && playerMeshes[i] != null)
                WinnerMeshes.Add(playerMeshes[i]);
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

		UpdateGlobalColorScheme();
		roundCount++;
		IngameUIManager.Instance.UpdateUI(teamPoints, roundCount);

		if (roundCount > matchSettings.Rounds)
		{
			BackToMenu();
			return;
		}
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