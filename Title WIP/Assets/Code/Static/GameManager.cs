using EZCameraShake;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;

public class GameManager
{
	#region Singleton Implementation
	private GameManager() => globalCamShake = CameraShaker.Instance.StartShake(1f, 1f, 0f);
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

	private float _shakeMagnitude;
	public float ShakeMagnitude
	{
		get => _shakeMagnitude;
		set => globalCamShake.ScaleMagnitude = _shakeMagnitude = value;
	}

	public float ShakeRoughness { set => globalCamShake.ScaleRoughness = value; }

	public float ShakeMagnitudeDecline { get; set; }
	#endregion

	public readonly List<int> registeredControlDeviceIDs = new List<int>();
	private readonly List<PlayerCharacter> registeredPlayerCharacters = new List<PlayerCharacter>();
	private CameraShakeInstance globalCamShake;

	private void RegisterControlDevice(InputAction.CallbackContext ctx)
	{
		if(!registeredControlDeviceIDs.Contains(ctx.control.device.id))
			registeredControlDeviceIDs.Add(ctx.control.device.id); // register new device ID
	}

	internal void RegisterBootstrapper(GameManagerBootstrapper bootstrapper) => this.bootstrapper = bootstrapper;
	internal void UnregisterBootstrapper() => bootstrapper = null;

	internal void ExtendedUpdate()
	{
		ShakeMagnitude = ShakeMagnitude > 0 ? ShakeMagnitude - ShakeMagnitudeDecline * Time.deltaTime : 0f;
	}

	public void RegisterPlayerCharacter(PlayerCharacter playerCharacter) => registeredPlayerCharacters.Add(playerCharacter);
	public bool UnregisterPlayerCharacter(PlayerCharacter playerCharacter) => registeredPlayerCharacters.Remove(playerCharacter);

	public PlayerCharacter RequestNearestPlayer(PlayerCharacter requestSender) // TODO improve this or dad will get the belt
		=> registeredPlayerCharacters[registeredPlayerCharacters.IndexOf(requestSender) == 0 ? 1 : 0];
	
}
