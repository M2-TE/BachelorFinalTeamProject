using UnityEngine;

public class MusicManagerBootstrapper : Bootstrapper
{
	[SerializeField] internal AudioContainer cont;

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