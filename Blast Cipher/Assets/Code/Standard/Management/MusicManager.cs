using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class MusicManager : Manager<MusicManager>
{
	public delegate void OnBeatCallback();


	private MusicManagerBootstrapper bootstrapper;
	private readonly LinkedList<OnBeatCallback> list = new LinkedList<OnBeatCallback>();
	private AudioSource source;
	private AudioContainer trackContainer;
	private int currentActiveTrack = 0;

	internal void RegisterBootstrapper(MusicManagerBootstrapper bootstrapper)
	{
		this.bootstrapper = bootstrapper;
		source = bootstrapper.GetComponent<AudioSource>();
	}

	private IEnumerator MusicHandler()
	{
		float bpm = trackContainer.bpmValues[currentActiveTrack];
		float timeBetweenBeats = 60f / bpm;
		var waiter = new WaitForSecondsRealtime(timeBetweenBeats);

		var clip = trackContainer.tracks[currentActiveTrack];

		source.clip = clip;
		source.Play();

		int currentBeat = 0;
		float targetTime = 0f;
		while (true)
		{
			bootstrapper.debugImage.enabled = !bootstrapper.debugImage.enabled;

			if (list.Count > 0)
			{
				list.First.Value?.Invoke();
				list.RemoveFirst();
			}

			targetTime = timeBetweenBeats * (++currentBeat);

			if (source.time + (targetTime - source.time) > source.clip.length)
			{
				currentBeat = 1;
				targetTime = timeBetweenBeats;
			}
			waiter = new WaitForSecondsRealtime(targetTime - source.time);
			yield return waiter;
		}
	}

	public void PlayMusic(AudioContainer tracks)
	{
		trackContainer = tracks;
		bootstrapper.StartCoroutine(MusicHandler());
	}

	public void RegisterCallOnNextBeat(OnBeatCallback callback, int beatsToSkip = 0)
	{
		if (list.First == null)
			list.AddFirst(default(OnBeatCallback));

		var node = list.First;

		for (int i = 0; i < beatsToSkip; i++)
		{
			if (node.Next == null)
				list.AddLast(default(OnBeatCallback));

			node = node.Next;
		}

		node.Value += callback;
	}
}
