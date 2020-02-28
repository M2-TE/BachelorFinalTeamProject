using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class OneShotAudioManager : MonoBehaviour
{
	[SerializeField] private OneShotAudioSource prefab;

	private readonly Queue<OneShotAudioSource> queuedAudioSources = new Queue<OneShotAudioSource>();

	private void OnEnable() => instance = this;
	private void OnDisable() => instance = instance == this ? null : instance;

	private static OneShotAudioManager instance;
	public static OneShotAudioSource PlayOneShotAudio(AudioClip[] clips, Vector3 worldPos, float vol = 1f)
	{
		return PlayOneShotAudio(clips[Random.Range(0, clips.Length)], worldPos, vol);
	}
	public static OneShotAudioSource PlayOneShotAudio(AudioClip clip, Vector3 worldPos, float vol = 1f)
	{
		OneShotAudioSource audioSource = null;
		if (instance.queuedAudioSources.Count > 0)
		{
			audioSource = instance.queuedAudioSources.Dequeue();
			while(audioSource == null)
			{
				if(instance.queuedAudioSources.Count > 0)
				{
					audioSource = instance.queuedAudioSources.Dequeue();
				}
				else
				{
					audioSource = Instantiate(instance.prefab);
					//instance.StartCoroutine(DelayedEnqueue(audioSource, clip.length));
				}
			}
		}
		else
		{
			audioSource = Instantiate(instance.prefab);
		}
		instance.StartCoroutine(DelayedEnqueue(audioSource, clip.length));

		audioSource.transform.position = worldPos;
		audioSource.AudioSource.PlayOneShot(clip, vol);


		return audioSource;
	}

	private static IEnumerator DelayedEnqueue(OneShotAudioSource source, float waitDelay)
	{
		yield return new WaitForSecondsRealtime(waitDelay);
		instance.queuedAudioSources.Enqueue(source);
	}
}
