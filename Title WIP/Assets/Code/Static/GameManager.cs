using UnityEngine;

public class GameManager
{
	#region Singleton Implementation
	private GameManager() { }
	private static GameManager instance;
	public static GameManager Instance { get => instance ?? (instance = new GameManager()); }
	#endregion

	public Camera MainCam { get => bootstrapper?.MainCam; }
	public InputMaster InputMaster { get => bootstrapper?.InputMaster; }

	private GameManagerBootstrapper bootstrapper;

	internal void RegisterBootstrapper(GameManagerBootstrapper bootstrapper) => this.bootstrapper = bootstrapper;
	internal void UnregisterBootstrapper() => bootstrapper = null;
}
