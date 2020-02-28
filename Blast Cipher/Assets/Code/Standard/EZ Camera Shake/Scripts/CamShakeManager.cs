using EZCameraShake;
using UnityEngine;

public sealed class CamShakeManager : Manager<CamShakeManager>
{
	public CamShakeManager() { }

	public CameraShakeInstance globalCamShake;

	private float _shakeMagnitude;
	public float ShakeMagnitude
	{
		get => _shakeMagnitude;
		set => globalCamShake.ScaleMagnitude = _shakeMagnitude = value;
	}

	public float ShakeRoughness { set => globalCamShake.ScaleRoughness = value; }

	public float ShakeMagnitudeDecline { get; set; }

	protected override void ExtendedUpdate() => ShakeMagnitude = ShakeMagnitude > 0 ? ShakeMagnitude - ShakeMagnitudeDecline * Time.deltaTime : 0f;
}