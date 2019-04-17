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
		SetActiveMusicPlayer(currentActiveMusicPlayer > bootstrapper.audioSources.Length ? 0 : currentActiveMusicPlayer + 1);
	}

	private float GetVolume(int index) => index == currentActiveMusicPlayer ? bootstrapper.maxVolume : 0f;

	protected override void ExtendedUpdate()
	{
		Debug.Log(currentActiveMusicPlayer);
		if (Input.GetKeyDown(KeyCode.Space))
		{
			SwitchToNextIntensity();
		}
	}
}
