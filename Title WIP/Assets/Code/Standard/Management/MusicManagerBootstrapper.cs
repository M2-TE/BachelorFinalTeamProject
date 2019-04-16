using UnityEngine;

public class MusicManagerBootstrapper : Bootstrapper<MusicManager>
{
	private AudioSource ownAudioSource;
	private MusicManager musicManager;

	protected override void Awake()
	{
		base.Awake();
		ownAudioSource = GetComponent<AudioSource>();
	}
}