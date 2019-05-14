using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class EssentialsLoader
{
	private static bool sceneLoaded = false;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	static void AfterSceneLoad()
	{

	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void BeforeSceneLoad()
	{
		if (!sceneLoaded)
		{
			var sceneParams = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.None);
			GameManager.Instance.SetAsyncEssentialsScene(SceneManager.LoadScene("AsyncEssentials", sceneParams));
			sceneLoaded = true;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	static void AfterAssembliesLoaded()
	{

	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	static void BeforeSplashScreen()
	{

	}
}
