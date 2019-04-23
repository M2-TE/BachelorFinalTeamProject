using UnityEngine;

[CreateAssetMenu(fileName = "PlayerCharacterSettings", menuName = "Settings/PlayerCharacter", order = 0)]
public class PlayerCharacterSettings : ReadonlySettings
{
	public Projectile ProjectilePrefab;

	[Header("Movement")]
	//public float gravityMod DEPRECATED
	public float MovespeedMod = 8f;
	public float DashCooldown = 5f;
	public float DashDuration = .2f;
	public float DashSpeed = 30f;
	public float DashAfterimagePadding = .05f;

	[Header("Combat")]
	public float ParryCooldown = 5f;
	public float ShotCooldown = .2f;
	public float ShotStrength = 30f;
	public float PowerUpDuration = 10f;

	[Header("Projectiles")]
	public Vector3 OrbitDelta = new Vector3(0f, 100f, 0f);
	public float MaxOrbitDist = 10f;
	public float MovementOrbit = 2.5f;
	public float MovementInterpolation = 0.7f;
	public float OrbitDist = 1f;
	public float RotationMin = 10f;
	public float RotationMax = 100f;

	[Header("Screen Shake")]
	public float ShotShakeMagnitude = .35f;
	public float DeathShakeMagnitude = 1f;

	[Header("Misc")]
	public float AimLineLengthMax = 3f;
}