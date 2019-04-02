﻿using UnityEngine;

public class GameManagerBootstrapper : MonoBehaviour
{
	[SerializeField] internal Camera mainCam;

	private void OnEnable() => GameManager.Instance.RegisterBootstrapper(this);
	private void OnDisable() => GameManager.Instance.UnregisterBootstrapper();

	private void Awake()
	{
		Cursor.lockState = CursorLockMode.Confined;
	}

	private void Start()
	{
		
	}
}