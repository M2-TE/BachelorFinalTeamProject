using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class EssentialsLoader
{
	private static bool sceneLoaded = false;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	static void AfterSceneLoad()
	{
		Debug.Log("After Scene Load");
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void BeforeSceneLoad()
	{
		if(!sceneLoaded)
		{
			SceneManager.LoadScene("AsyncEssentials", LoadSceneMode.Additive);
			sceneLoaded = true;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	static void AfterAssembliesLoaded()
	{
		Debug.Log("After Assemblies Loaded");
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	static void BeforeSplashScreen()
	{
		Debug.Log("Before Splash Screen");
	}
}
