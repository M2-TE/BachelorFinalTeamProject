using UnityEngine;

public class MusicManagerBootstrapper : Bootstrapper
{
	[SerializeField] internal AudioSource[] audioSources;
	[SerializeField] internal float[] debugBPMs;

	[Space]
	[SerializeField] internal AnimationCurve fadeInCurve;
	[SerializeField] internal AnimationCurve fadeOutCurve;
	[SerializeField] internal float maxVolume = .2f;
	[SerializeField] internal float fadeDuration = 1f;
	
	private void Awake()
	{
		MusicManager.Instance.RegisterBootstrapper(this);
	}
}