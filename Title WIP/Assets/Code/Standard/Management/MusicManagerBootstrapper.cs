using UnityEngine;

public class MusicManagerBootstrapper : Bootstrapper
{
	[SerializeField] internal AudioSource[] audioSources;
	[SerializeField] internal float maxVolume = .2f;
	
	private void Awake()
	{
		MusicManager.Instance.RegisterBootstrapper(this);
	}
}