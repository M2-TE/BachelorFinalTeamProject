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
	private int transitions = 0;
	private int currentActiveTrack = 0;
	private int currentBeat = 0;
	private float targetTime = 0f;
	private float timeBetweenBeats = 0f;
	private bool intensitySwitchBuffered = false;
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
				if (intensitySwitchBuffered)
				{
					// get playback position
					float bufferedTime = source.time;

					// switch to next intensity int
					currentActiveTrack = (currentActiveTrack + 1) % trackContainer.tracks.Length;
					
					// callback
					bufferedTransitionCall();

					if (transitions % 3 == 0)
					{
						// play transition
						source.volume = .2f;
						source.PlayOneShot(trackContainer.TransitionTrack[currentActiveTrack], 5f);
						//source.clip = trackContainer.TransitionTrack[currentActiveTrack];
						//source.time = 0f;
						//source.Play();
						
						yield return new WaitForSecondsRealtime(trackContainer.TransitionTrack[currentActiveTrack].length);
						source.volume = 1f;
					}
					else
					{
						// out smoothing
						var snapshot = bootstrapper.musicMixer.FindSnapshot("RoundEnding");
						snapshot = bootstrapper.musicMixer.FindSnapshot("Main");
						snapshot.TransitionTo(4f * timeBetweenBeats);
					}


					// switch to next intensity track
					clip = trackContainer.tracks[currentActiveTrack];
					source.clip = clip;

					// calc new time buffer (due to bpm difference)
					currentBeat = (int)(bufferedTime / timeBetweenBeats);
					timeBetweenBeats = 60f / trackContainer.bpmValues[currentActiveTrack];
					bufferedTime = currentBeat * timeBetweenBeats /*+ timeBetweenBeats * .025f*/;
					bootstrapper.StartCoroutine(SmoothVolIn());

					// set new track 
					source.time = bufferedTime;
					source.Play();

					// retoggle main beat since this iteration of the loop is going to be skipped
					bootstrapper.debugImage.enabled = !bootstrapper.debugImage.enabled;

					intensitySwitchBuffered = false;
					transitions++;
					continue;
				}
				else
				{
					// toggle bar
					bootstrapper.debugImageTwo.enabled = !bootstrapper.debugImageTwo.enabled;
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

	private IEnumerator SmoothVolIn()
	{
		source.volume *= .5f;
		float targetTime = source.time + .25f;
		int calls = 0;
		do
		{
			source.volume = Mathf.MoveTowards(source.volume, 1f, .1f);
			calls++;
			yield return null;
		} while (source.time < targetTime);
		source.volume = 1f;
		Debug.Log(calls);
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

	public void TransitionToNextIntensity(OnBeatCallback onTransitionCallback)
	{
		bootstrapper.StartCoroutine(TransitionEffect(onTransitionCallback));
	}

	private IEnumerator TransitionEffect(OnBeatCallback onTransitionCallback)
	{
		// waiter until next bar with a minimum wait time of a 4 beats/1 bar
		float timeUntilNextBar = GetTimeUntilNextBar();
		timeUntilNextBar += 4f * timeBetweenBeats;

		if(timeUntilNextBar < 6f * timeBetweenBeats)
		{
			timeUntilNextBar += 4f * timeBetweenBeats;
		}
		else if(timeUntilNextBar > 8f * timeBetweenBeats)
		{
			timeUntilNextBar -= 4f * timeBetweenBeats;
		}

		//float timeToWait = GetTimeUntilNextBar() + 4f * timeBetweenBeats;

		if(transitions % 3 != 0)
		{
			// in smoothing
			var snapshot = bootstrapper.musicMixer.FindSnapshot("RoundEnding");
			snapshot.TransitionTo(timeUntilNextBar);
		}

		// buffer switch to next intensity for the music handler
		yield return new WaitForSecondsRealtime(timeUntilNextBar - 2f * timeBetweenBeats);
		intensitySwitchBuffered = true;
		bufferedTransitionCall = onTransitionCallback;

		//call the transition callback on the full bar
		//yield return new WaitForSecondsRealtime(2f * timeBetweenBeats);
		//onTransitionCallback();
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
