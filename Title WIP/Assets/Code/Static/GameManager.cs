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

	#region Properties
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
	#endregion

	public readonly List<int> registeredControlDeviceIDs = new List<int>();
	private readonly List<PlayerCharacter> registeredPlayerCharacters = new List<PlayerCharacter>();

	internal void RegisterBootstrapper(GameManagerBootstrapper bootstrapper) => this.bootstrapper = bootstrapper;
	internal void UnregisterBootstrapper() => bootstrapper = null;

	public void RegisterPlayerCharacter(PlayerCharacter playerCharacter) => registeredPlayerCharacters.Add(playerCharacter);
	public bool UnregisterPlayerCharacter(PlayerCharacter playerCharacter) => registeredPlayerCharacters.Remove(playerCharacter);

	public PlayerCharacter RequestNearestPlayer(PlayerCharacter requestSender) // TODO make this good or dad will get the belt
		=> registeredPlayerCharacters[registeredPlayerCharacters.IndexOf(requestSender) == 0 ? 1 : 0];

	private void RegisterControlDevice(InputAction.CallbackContext ctx)
	{
		if(!registeredControlDeviceIDs.Contains(ctx.control.device.id))
			registeredControlDeviceIDs.Add(ctx.control.device.id); // register new device ID
	}
}
