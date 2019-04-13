using EZCameraShake;
using UnityEngine;

public sealed class CamShakeManager : Manager
{
	private CamShakeManager() : base()
	{
		globalCamShake = CameraShaker.Instance.StartShake(1f, 1f, 0f);
	}
	private static CamShakeManager instance;
	public static CamShakeManager Instance { get => instance ?? (instance = new CamShakeManager()); }

	private CameraShakeInstance globalCamShake;

	#region Global Shake Properties
	private float _shakeMagnitude;
	public float ShakeMagnitude
	{
		get => _shakeMagnitude;
		set => globalCamShake.ScaleMagnitude = _shakeMagnitude = value;
	}

	public float ShakeRoughness { set => globalCamShake.ScaleRoughness = value; }

	public float ShakeMagnitudeDecline { get; set; }
	#endregion

	protected override void ExtendedUpdate() => ShakeMagnitude = ShakeMagnitude > 0 ? ShakeMagnitude - ShakeMagnitudeDecline * Time.deltaTime : 0f;
}