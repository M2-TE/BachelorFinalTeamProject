﻿using System.Collections;
using UnityEngine;

public sealed class MusicManager : Manager<MusicManager>
{
	private MusicManagerBootstrapper bootstrapper;
	private int currentActiveMusicPlayer;
	
	internal void RegisterBootstrapper(MusicManagerBootstrapper bootstrapper)
	{
		this.bootstrapper = bootstrapper;

		SetActiveMusicPlayer(0);
	}

	private void SetActiveMusicPlayer(int index)
	{
		currentActiveMusicPlayer = index;
		bootstrapper.audioSources[0].volume = GetVolume(0);
		bootstrapper.audioSources[1].volume = GetVolume(1);
		bootstrapper.audioSources[2].volume = GetVolume(2);
	}

	private void SwitchToNextIntensity()
	{
		SetActiveMusicPlayer(currentActiveMusicPlayer == bootstrapper.audioSources.Length - 1 ? 0 : currentActiveMusicPlayer + 1);
	}

	private float GetVolume(int index) => index == currentActiveMusicPlayer ? bootstrapper.maxVolume : 0f;

	protected override void ExtendedUpdate()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			//SwitchToNextIntensity();
			bootstrapper.StartCoroutine(FadeToNext());
		}
	}

	private IEnumerator FadeToNext()
	{
		float timer = 0f;
		float volume = bootstrapper.maxVolume;
		AnimationCurve fadeIn = bootstrapper.fadeInCurve;
		AnimationCurve fadeOut = bootstrapper.fadeOutCurve;

		AudioSource fadeOutSource = bootstrapper.audioSources[currentActiveMusicPlayer];
		currentActiveMusicPlayer = currentActiveMusicPlayer == bootstrapper.audioSources.Length - 1 ? 0 : currentActiveMusicPlayer + 1;
		AudioSource fadeInSource = bootstrapper.audioSources[currentActiveMusicPlayer];


		while (timer < fadeIn.keys[fadeIn.length - 1].time)
		{
			fadeInSource.volume = fadeIn.Evaluate(timer) * volume;
			fadeOutSource.volume = fadeOut.Evaluate(timer) * volume;
			timer += Time.deltaTime;
			yield return null;
		}
		fadeInSource.volume = volume;
		fadeOutSource.volume = 0f;
		
	}
}