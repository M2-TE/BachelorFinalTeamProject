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
	private OnBeatCallback bufferedTransitionCall;

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
				
				// invoke OnBar callbacks
				if (onBarCallbacks.Count > 0)
				{
					onBarCallbacks.First.Value?.Invoke();
					onBarCallbacks.RemoveFirst();
				}

				// check if next intensity should be switched to
				//if (intensitySwitchBuffered)
				//{
				//	// get playback position
				//	float bufferedTime = source.time;
					
				//	// callback
				//	bufferedTransitionCall();

				//	// out smoothing
				//	var snapshot = bootstrapper.musicMixer.FindSnapshot("Main");
				//	snapshot.TransitionTo(4f * timeBetweenBeats);

				//	// switch to next intensity int
				//	currentActiveTrack = (currentActiveTrack + 1) % trackContainer.tracks.Length;

				//	// play transition
				//	source.volume = .2f;
				//	source.PlayOneShot(trackContainer.TransitionTrack[currentActiveTrack], 5f);
				//	Effects.StartVignetteTransition(1f, .1f);

				//	yield return new WaitForSecondsRealtime(trackContainer.TransitionTrack[currentActiveTrack].length);
				//	source.volume = 1f;

				//	FadeOutCalls();

				//	// switch to next intensity track
				//	clip = trackContainer.tracks[currentActiveTrack];
				//	source.clip = clip;

				//	// calc new time buffer (due to bpm difference)
				//	//currentBeat = (int)(bufferedTime / timeBetweenBeats);
				//	//timeBetweenBeats = 60f / trackContainer.bpmValues[currentActiveTrack];
				//	bufferedTime = /*(currentBeat - 2) * timeBetweenBeats*/0f;
				//	currentBeat = 0;
				//	bootstrapper.StartCoroutine(SmoothVolIn());

				//	// set new track 
				//	source.time = bufferedTime;
				//	source.Play();

				//	// retoggle main beat since this iteration of the loop is going to be skipped
				//	bootstrapper.debugImage.enabled = !bootstrapper.debugImage.enabled;

				//	intensitySwitchBuffered = false;
				//	continue;
				//}
				//else
				//{
				//	// toggle bar
				//	bootstrapper.debugImageTwo.enabled = !bootstrapper.debugImageTwo.enabled;
				//}
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

	private IEnumerator SmoothVolIn()
	{
		source.volume *= .5f;
		float targetTime = source.time + .25f;
		do
		{
			source.volume = Mathf.MoveTowards(source.volume, 1f, .1f);
			yield return null;
		} while (source.time < targetTime);
		source.volume = 1f;
	}

	private float GetTimeUntilNextBeat()
	{
		return default;
	}

	private float GetTimeUntilNextBar()
	{
		return (currentBeat % 4) * timeBetweenBeats + targetTime - (source.time/* - 0.01f float precision smoothing */);
	}

	public void PlayMusic(AudioContainer tracks)
	{
		trackContainer = tracks;
		bootstrapper.StartCoroutine(MusicHandler());
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
			source.PlayOneShot(bootstrapper.cont.TransitionTrack[0], 2f);
			currentActiveTrack = (currentActiveTrack + 1) % trackContainer.tracks.Length;
		}

		var snapshot = bootstrapper.musicMixer.FindSnapshot("RoundEnding");
		snapshot.TransitionTo(standardInDuration);
		
		yield return new WaitForSecondsRealtime(standardInDuration);
		onTransitionCallback();

		if (transitionIntensity)
		{
			source.clip = bootstrapper.cont.tracks[currentActiveTrack];
			source.Play();
		}

		snapshot = bootstrapper.musicMixer.FindSnapshot("Main");
		snapshot.TransitionTo(standardOutDuration);

		FadeOutCalls(1f);
	}

	private IEnumerator TransitionEffect(OnBeatCallback onTransitionCallback)
	{
		// waiter until next bar with a minimum wait time of a 4 beats/1 bar
		float timeUntilNextBar = GetTimeUntilNextBar();
		timeUntilNextBar += 4f * timeBetweenBeats;

		if(timeUntilNextBar < 5f * timeBetweenBeats)
		{
			timeUntilNextBar += 4f * timeBetweenBeats;
			Debug.Log("lengthened");
		}
		else if(timeUntilNextBar > 11f * timeBetweenBeats)
		{
			timeUntilNextBar -= 4f * timeBetweenBeats;
			Debug.Log("shortened");
		}

		// set target vignette
		FadeInCalls(timeUntilNextBar);

		// in smoothing
		var snapshot = bootstrapper.musicMixer.FindSnapshot("RoundEnding");
		snapshot.TransitionTo(timeUntilNextBar * .9f);

		// buffer switch to next intensity for the music handler
		yield return new WaitForSecondsRealtime(timeUntilNextBar - 2f * timeBetweenBeats);
		bufferedTransitionCall = onTransitionCallback;
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
