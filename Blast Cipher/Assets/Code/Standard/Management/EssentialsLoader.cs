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
			SceneManager.LoadScene("AsyncEssentials", LoadSceneMode.Additive);
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
