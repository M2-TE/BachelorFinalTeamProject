using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;

public class GameManager
{
	#region Singleton Implementation
	private GameManager() { }
	private static GameManager instance;
	public static GameManager Instance { get => instance ?? (instance = new GameManager()); }
	#endregion

	private GameManagerBootstrapper _bootstrapper;
	private GameManagerBootstrapper bootstrapper
	{
		get => _bootstrapper;
		set
		{
			if (value != null)
				value.InputMaster.General.RegisterDevice.performed += RegisterControlDevice;
			else
				_bootstrapper.InputMaster.General.RegisterDevice.performed -= RegisterControlDevice;

			_bootstrapper = value;
		}
	}

	public Camera MainCam { get => bootstrapper?.MainCam; }
	public InputMaster InputMaster { get => bootstrapper?.InputMaster; }

	public readonly List<int> registeredControlDeviceIDs = new List<int>();

	internal void RegisterBootstrapper(GameManagerBootstrapper bootstrapper) => this.bootstrapper = bootstrapper;
	internal void UnregisterBootstrapper() => bootstrapper = null;

	private void RegisterControlDevice(InputAction.CallbackContext ctx)
	{
		if(!registeredControlDeviceIDs.Contains(ctx.control.device.id))
			registeredControlDeviceIDs.Add(ctx.control.device.id); // register new device ID
	}
}
