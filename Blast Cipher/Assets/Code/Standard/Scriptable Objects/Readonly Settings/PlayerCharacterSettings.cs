using UnityEngine;

[CreateAssetMenu(fileName = "PlayerCharacterSettings", menuName = "Settings/PlayerCharacter", order = 0)]
public class PlayerCharacterSettings : ReadonlySettings
{
	public InputMaster InputMaster;
	public Projectile ProjectilePrefab;
	public Portal PortalPrefab;

	[Header("Audio")]
	public AudioClip[] ProjectileShotSounds;
	public AudioClip[] PlayerDeathSounds;

	[Header("Movement")]
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
	public int startProjectileCount = 3;
	public float projectileRespawnTimer = 5f;
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
	public string WallTag = "Wall";
	public LayerMask WallLayers;

	public string OuterWallTag = "OuterWall";
	public LayerMask OuterWallLayer;

	[Space]
	public LayerMask ProjectileLayer;
	public float ProjectileMagnetRadius;
	public float ProjectileMagnetForce;

	[Space]
	public LayerMask TeleportCompatibleLayers;
	public float AimLineLengthMax = 3f;
}