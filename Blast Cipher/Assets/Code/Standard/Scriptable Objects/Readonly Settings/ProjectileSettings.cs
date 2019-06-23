using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileSettings", menuName = "Settings/Projectile", order = 1)]
public class ProjectileSettings : ReadonlySettings
{
	public GameObject ExplosionPrefab;
	public AudioClip[] wallBounceSounds;
	public AudioClip[] wallHitSounds;
	public AudioClip[] explosionSounds;

	public string PlayerTag = "Player";
	public string WallTag = "Wall";
	public string OuterWallTag = "OuterWall";
	public string ShieldWallTag = "ParryShield";

	public float MaxRadiansOnAutoTarget = 7f;
	public float VelocityChangeOnWallHit = .1f;

	[Header("Materials")]
	public Material StandardProjectileMaterial;

	public Material BounceCubeMaterial;
	public Material ExplosionCubeMaterial;
	public Material AutoAimCubeMaterial;

	public Material BounceAndExplosionCombinedMaterial;
	public Material AutoAimAndBounceCombinedMaterial;
	public Material AutoAimAndExplosionCombinedMaterial;
}
