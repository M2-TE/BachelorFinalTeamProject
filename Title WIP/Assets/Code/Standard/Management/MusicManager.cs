using System.Collections;
using UnityEngine;

public sealed class MusicManager : Manager<MusicManager>
{
	private MusicManagerBootstrapper bootstrapper;
	private int currentActiveMusicPlayer;

	private int currentActiveTrack = 0;
	private bool trackSwitchBuffered = false;

	private float waitTime { get => 60f / bootstrapper.debugBPMs[currentActiveTrack] * 16f - (60f / bootstrapper.debugBPMs[currentActiveTrack] * 16f) * .0075f; }

	internal void RegisterBootstrapper(MusicManagerBootstrapper bootstrapper)
	{
		this.bootstrapper = bootstrapper;
		bootstrapper.StartCoroutine(TrackHandler());
	}

	private IEnumerator TrackHandler()
	{
		Debug.Log(waitTime);
		var waiter = new WaitForSeconds(waitTime);
		bootstrapper.audioSources[currentActiveTrack].Play();

		while (true)
		{
			if (!trackSwitchBuffered) yield return waiter;
			else
			{
				bootstrapper.audioSources[currentActiveTrack].Stop();
				bootstrapper.audioSources[++currentActiveTrack].Play();
				waiter = new WaitForSeconds(waitTime);
				trackSwitchBuffered = false;
			}
		}
	}

	protected override void ExtendedUpdate()
	{
		if (Input.GetKeyDown(KeyCode.Space)) trackSwitchBuffered = true;
	}
}
