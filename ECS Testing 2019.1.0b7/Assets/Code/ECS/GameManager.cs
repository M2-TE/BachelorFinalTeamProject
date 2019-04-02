using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
	#region Singleton Implementation
	private GameManager() { }
	private static GameManager instance;
	public static GameManager Instance
	{
		get { return instance ?? (instance = new GameManager()); }
	}
	#endregion

	public GameManagerBootstrapper bootstrapper;

	public void RegisterBootstrapper(GameManagerBootstrapper bootstrapper)
	{
		this.bootstrapper = bootstrapper;
	}

	public void UnregisterBootstrapper() { bootstrapper = null; }
}
