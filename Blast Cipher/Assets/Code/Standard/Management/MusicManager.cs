using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class MusicManager : Manager<MusicManager>
{
	public delegate void OnBeatCallback();
	
	private MusicManagerBootstrapper bootstrapper;
	private AudioSource source;
	private AudioContainer trackContainer;

	private readonly LinkedList<OnBeatCallback> onBeatCallbacks = new LinkedList<OnBeatCallback>();
	private readonly LinkedList<OnBeatCallback> onBarCallbacks = new LinkedList<OnBeatCallback>();
	private int currentActiveTrack = 0;
	private int currentBeat = 0;
	private float targetTime = 0f;
	private float timeBetweenBeats = 0f;
	private bool intensitySwitchBuffered = false;

	internal void RegisterBootstrapper(MusicManagerBootstrapper bootstrapper)
	{
		this.bootstrapper = bootstrapper;
		source = bootstrapper.GetComponent<AudioSource>();
	}

	private IEnumerator MusicHandler()
	{
		timeBetweenBeats = 60f / trackContainer.bpmValues[currentActiveTrack];
		var waiter = new WaitForSecondsRealtime(timeBetweenBeats);

		var clip = trackContainer.tracks[currentActiveTrack];

		source.clip = clip;
		source.Play();

		while (true)
		{
			// on every beat
			{
				// show beat
				bootstrapper.debugImage.enabled = !bootstrapper.debugImage.enabled;

				// invoke OnBeat callbacks
				if (onBeatCallbacks.Count > 0)
				{
					onBeatCallbacks.First.Value?.Invoke();
					onBeatCallbacks.RemoveFirst();
				}
			}

			// on every bar (bar = 4 beats on 4/4 rhythm)
			if(currentBeat % 4 == 0)
			{
				// show bar
				bootstrapper.debugImageTwo.enabled = !bootstrapper.debugImageTwo.enabled;
				
				// invoke OnBar callbacks
				if (onBarCallbacks.Count > 0)
				{
					onBarCallbacks.First.Value?.Invoke();
					onBarCallbacks.RemoveFirst();
				}

				// check if next intensity should be switched to
				if (intensitySwitchBuffered)
				{
					currentActiveTrack = (currentActiveTrack + 1) % trackContainer.tracks.Length;
					Debug.Log(currentActiveTrack);

					clip = trackContainer.tracks[currentActiveTrack];
					source.clip = clip;
					source.Play();

					currentBeat = 0;
					timeBetweenBeats = 60f / trackContainer.bpmValues[currentActiveTrack];

					intensitySwitchBuffered = false;
					continue;
				}
			}

			// calc new targetTime
			targetTime = timeBetweenBeats * (++currentBeat);

			// handle music looping
			if (source.time + (targetTime - source.time) > source.clip.length)
			{
				currentBeat = 1;
				targetTime = timeBetweenBeats;
			}

			// wait until next beat
			waiter = new WaitForSecondsRealtime(targetTime - source.time);
			yield return waiter;
		}
	}

	private float GetTimeUntilNextBeat()
	{
		return default;
	}

	private float GetTimeUntilNextBar()
	{
		return (currentBeat % 4) * timeBetweenBeats + targetTime - source.time;
	}

	public void PlayMusic(AudioContainer tracks)
	{
		trackContainer = tracks;
		bootstrapper.StartCoroutine(MusicHandler());
	}

	public void TransitionToNextIntensity(OnBeatCallback onTransitionCallback)
	{
		bootstrapper.StartCoroutine(TransitionEffect(onTransitionCallback));
	}

	private IEnumerator TransitionEffect(OnBeatCallback onTransitionCallback)
	{
		var snapshot = bootstrapper.musicMixer.FindSnapshot("RoundEnding");
		float timeToWait = GetTimeUntilNextBar() + 4f * timeBetweenBeats;
		snapshot.TransitionTo(timeToWait);
		Debug.Log(timeToWait);

		yield return new WaitForSecondsRealtime(GetTimeUntilNextBar() + 2f * timeBetweenBeats);
		intensitySwitchBuffered = true;

		yield return new WaitForSecondsRealtime(2f * timeBetweenBeats);
		onTransitionCallback();

		snapshot = bootstrapper.musicMixer.FindSnapshot("Main");
		snapshot.TransitionTo(2f);
	}

	public void RegisterCallOnNextBeat(OnBeatCallback callback, int beatsToSkip = 0, bool onBar = false)
	{
		var callbacks = onBar ? onBarCallbacks : onBeatCallbacks;

		if (callbacks.First == null)
		{
			callbacks.AddFirst(default(OnBeatCallback));
		}
		var node = callbacks.First;

		for (int i = 0; i < beatsToSkip; i++)
		{
			if (node.Next == null)
			{
				callbacks.AddLast(default(OnBeatCallback));
			}
			node = node.Next;
		}

		node.Value += callback;
	}
}
