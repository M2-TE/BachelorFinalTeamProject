using System.Collections;
using UnityEngine;

public sealed class MusicManager : Manager<MusicManager>
{
	private MusicManagerBootstrapper bootstrapper;
	private int currentActiveMusicPlayer;

	private int currentActiveTrack = 0;
	private bool trackSwitchBuffered = false;

	private float trackSwitchSmoothingVal = .15f;
	private float trackSwitchTime { get => trackSwitchSmoothingVal * (60f / bootstrapper.debugBPMs[currentActiveTrack] * 4f); }
	private float waitTime { get => 60f / bootstrapper.debugBPMs[currentActiveTrack] * 4f; }

	internal void RegisterBootstrapper(MusicManagerBootstrapper bootstrapper)
	{
		this.bootstrapper = bootstrapper;
		bootstrapper.StartCoroutine(TrackHandler());
	}

	private IEnumerator TrackHandler()
	{
		var waiter = new WaitForSeconds(waitTime - trackSwitchTime);
		var trackSwitchWaiter = new WaitForSeconds(trackSwitchTime);
		bootstrapper.audioSources[currentActiveTrack].Play();

		while (true)
		{
			yield return waiter;
			if (trackSwitchBuffered)
			{
				bootstrapper.audioSources[currentActiveTrack].Stop();
				bootstrapper.audioSources[currentActiveTrack == bootstrapper.debugBPMs.Length ? 0 : currentActiveTrack + 1].Play();

				trackSwitchBuffered = false;
				waiter = new WaitForSeconds(waitTime - trackSwitchTime);
				trackSwitchWaiter = new WaitForSeconds(trackSwitchTime);
				continue;
			}
			yield return trackSwitchWaiter;

			//if (!trackSwitchBuffered) yield return waiter;
			//else
			//{
			//	bootstrapper.audioSources[currentActiveTrack].Stop();
			//	bootstrapper.audioSources[++currentActiveTrack].Play();
			//	waiter = new WaitForSeconds(waitTime);
			//	trackSwitchBuffered = false;
			//}
		}
	}

	protected override void ExtendedUpdate()
	{
		if (Input.GetKeyDown(KeyCode.Space)) trackSwitchBuffered = true;
	}
}
