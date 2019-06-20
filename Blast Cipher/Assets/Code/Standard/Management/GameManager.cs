using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public sealed class GameManager
{
	#region Singleton Implementation
	private GameManager() { }
	private static GameManager instance;
	public static GameManager Instance { get => instance ?? (instance = new GameManager()); }
	#endregion

	#region Properties
	private GameManagerBootstrapper _bootstrapper;
	private GameManagerBootstrapper bootstrapper;

	public readonly InputDevice[] inputDevices = new InputDevice[2];
	public bool playerInputsActive = true;

	private int roundCount;
	private bool nextRoundStarterInProgress = false;
	private Scene asyncEssentials;
	private Scene currentMainScene;
	#endregion

	public delegate void ExtendedUpdate();

	public readonly List<ExtendedUpdate> extendedUpdates = new List<ExtendedUpdate>();
	private readonly List<PlayerCharacter> registeredPlayerCharacters = new List<PlayerCharacter>(2);
	private readonly List<GameObject> temporaryObjects = new List<GameObject>();

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
			nextRoundStarterInProgress = true;
			//playerInputsActive = false;
			if (roundCount != 0 && roundCount % 3 == 0)
			{
				MusicManager.Instance.TransitionToNextIntensity(OnNextMusicBar);
			}
			else
			{
				MusicManager.Instance.RoundTransitionSmoother(OnNextMusicBar);
			}
			bootstrapper.StartCoroutine(TimeScalerOnRoundTransition());

			roundCount++;
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