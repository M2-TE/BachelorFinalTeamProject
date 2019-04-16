using UnityEngine;

public sealed class MusicManager : Manager
{
	private MusicManager() { }
	private MusicManager instance;
	public MusicManager Instance
	{
		get => instance ?? (instance = new MusicManager());
	}

	private MusicManagerBootstrapper bootstrapper;
	private AudioSource globalAudioSource;


	internal void RegisterBootstrapper()
	{

	}

	protected override void ExtendedUpdate()
	{

	}
}
