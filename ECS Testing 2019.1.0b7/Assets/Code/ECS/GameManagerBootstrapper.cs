using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerBootstrapper : MonoBehaviour
{
	public AudioSource audioSource;
	public AudioListener audioListener;

	private void OnEnable()
	{
		GameManager.Instance.RegisterBootstrapper(this);
	}

	private void OnDisable()
	{
		GameManager.Instance.UnregisterBootstrapper();
	}
}
