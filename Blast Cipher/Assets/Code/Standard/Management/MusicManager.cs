using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class MusicManager : Manager<MusicManager>
{
	public delegate void OnBeatCallback();
	
	public AudioSource Source;
	private MusicManagerBootstrapper bootstrapper;
	private AudioContainer trackContainer;

	private readonly LinkedList<OnBeatCallback> onBeatCallbacks = new LinkedList<OnBeatCallback>();
	private readonly LinkedList<OnBeatCallback> onBarCallbacks = new LinkedList<OnBeatCallback>();
	private int currentActiveTrack = 0;
	private int currentBeat = 0;
	private float targetTime = 0f;
	private float timeBetweenBeats = 0f;
	private bool musicPlaying = false;
	private OnBeatCallback bufferedTransitionCall;

	internal void RegisterBootstrapper(MusicManagerBootstrapper bootstrapper)
	{
		this.bootstrapper = bootstrapper;
		Source = bootstrapper.GetComponent<AudioSource>();
	}

	private IEnumerator MusicHandler()
	{
		musicPlaying = true;
		WaitForSecondsRealtime waiter;

		while (true)
		{
			// on every beat
			{
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
				// invoke OnBar callbacks
				if (onBarCallbacks.Count > 0)
				{
					onBarCallbacks.First.Value?.Invoke();
					onBarCallbacks.RemoveFirst();
				}
			}

			// calc new targetTime
			targetTime = timeBetweenBeats * (++currentBeat);

			// handle music looping
			if (Source.time + (targetTime - Source.time) > Source.clip.length)
			{
				currentBeat = 1;
				targetTime = timeBetweenBeats;
			}

			// wait until next beat
			waiter = new WaitForSecondsRealtime(targetTime - Source.time);
			yield return waiter;
		}
	}

	public void PlayMusic(AudioContainer tracks)
	{
		trackContainer = tracks;

		currentBeat = 0;
		currentActiveTrack = 0;
		timeBetweenBeats = 60f / trackContainer.bpmValues[currentActiveTrack];

		Source.clip = trackContainer.tracks[currentActiveTrack];
		Source.Play();

		if (!musicPlaying)
		{
			bootstrapper.StartCoroutine(MusicHandler());
		}
	}

	public void RoundTransitionSmoother(OnBeatCallback onTransitionCallback, bool transitionIntensity)
	{
		bootstrapper.StartCoroutine(RoundTransitionSmoothingEffect(onTransitionCallback, transitionIntensity));
	}

	private IEnumerator RoundTransitionSmoothingEffect(OnBeatCallback onTransitionCallback, bool transitionIntensity)
	{
		float standardInDuration = 2.0f;
		float standardOutDuration = 3f;

		FadeInCalls(standardInDuration);

		if (transitionIntensity)
		{
			Source.PlayOneShot(bootstrapper.cont.TransitionTrack[0], 2f);
			currentActiveTrack = (currentActiveTrack + 1) % trackContainer.tracks.Length;
		}

		var snapshot = bootstrapper.musicMixer.FindSnapshot("RoundEnding");
		if (transitionIntensity)
		{
			snapshot.TransitionTo(standardInDuration);
		}
		
		yield return new WaitForSecondsRealtime(standardInDuration);
		onTransitionCallback();

		if (transitionIntensity)
		{
			currentBeat = 0;
			timeBetweenBeats = 60f / trackContainer.bpmValues[currentActiveTrack];
			Source.clip = bootstrapper.cont.tracks[currentActiveTrack];
			Source.Play();
		}

		snapshot = bootstrapper.musicMixer.FindSnapshot("Main");
		snapshot.TransitionTo(standardOutDuration);

		FadeOutCalls(1f);
	}

	public void LoadingScreenTransitionEffect(float duration, bool inTransition)
	{
		string snapshotString = inTransition ? "LoadingScreen" : "Main";
		var snapshot = bootstrapper.musicMixer.FindSnapshot(snapshotString);
		snapshot.TransitionTo(duration);
	}

	private void FadeInCalls(float duration)
	{
		Effects.StartVignetteTransition(.5f, duration);
		Effects.StartDigitalGlitchTransition(.35f, duration);
		Effects.StartAnalogGlitchTransition(.7f, .5f, 0f, .4f, duration);
	}

	private void FadeOutCalls(float duration)
	{
		Effects.ResetVignette(duration);
		Effects.ResetDigitalGlitch(duration);
		Effects.ResetAnalogGlitch(duration);
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
