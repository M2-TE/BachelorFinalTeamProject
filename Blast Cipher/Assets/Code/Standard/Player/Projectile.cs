using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, ITeleportable
{
	[SerializeField] private ProjectileSettings settings;
	
	private bool _canPickUp;
	public bool CanPickup
	{
		get => _canPickUp;
		set
		{
			_canPickUp = value;
			if (value) rgb.constraints = RigidbodyConstraints.None;
			else rgb.constraints = RigidbodyConstraints.FreezePositionY;
		}
	}

	private int _bounces;
	public int Bounces
	{
		get => _bounces;
		set
		{
			_bounces = value;
			UpdateActiveMaterial();
		}
	}

	private bool _explosive;
	public bool Explosive
	{
		get => _explosive;
		set
		{
			_explosive = value;
			UpdateActiveMaterial();
		}
	}

	private PlayerCharacter _explicitTarget = null;
	public PlayerCharacter ExplicitTarget
	{
		get => _explicitTarget;
		set
		{
			_explicitTarget = value;
			UpdateActiveMaterial();
		}
	}

	public Vector3 actualVelocity; // TODO update this value properly to handle portal physics
	public Rigidbody rgb;
	public new Collider collider;
	public new Renderer renderer;
	public PlayerCharacter InitialShooter;

	public bool CanBeTeleported { get; set; } = true;

	private void OnCollisionEnter(Collision collision)
	{
		var go = collision.gameObject;
		if (!CanPickup)
		{
			if (go.CompareTag(settings.PlayerTag))
			{
				go.GetComponent<PlayerCharacter>().TriggerDeath();
				Destroy(gameObject);
			}
			else if ((go.CompareTag(settings.WallTag) || go.CompareTag(settings.OuterWallTag)) && CanBeTeleported)
			{
				if (_explosive)
				{
					Instantiate(settings.ExplosionPrefab, collision.contacts[0].point, Quaternion.identity);
					OneShotAudioManager.PlayOneShotAudio(settings.explosionSounds, transform.position, 1f);
				}

				if (Bounces > 0)
				{
					Bounces--;
					actualVelocity = Vector3.Reflect(actualVelocity, collision.GetContact(0).normal);
					OneShotAudioManager.PlayOneShotAudio(settings.wallBounceSounds, transform.position, 1f);
				}
				else
				{
					if (_explosive) Explosive = false;
					else OneShotAudioManager.PlayOneShotAudio(settings.wallHitSounds, transform.position, 1f);

					rgb.angularVelocity = Vector3.zero;
					rgb.velocity = rgb.velocity.normalized * settings.VelocityChangeOnWallHit;
					CanPickup = true;
					ExplicitTarget = null;
				}
			}
			else if (go.CompareTag(settings.ShieldWallTag))
			{
				OneShotAudioManager.PlayOneShotAudio(settings.wallBounceSounds, transform.position, 1f);
			}
		}
	}

	private void Awake()
	{
		renderer = GetComponent<Renderer>();
	}

	private void Update()
	{
		if(ExplicitTarget != null && !CanPickup)
		{
			Vector3 targetVelocity = Vector3.RotateTowards
				(rgb.velocity, 
				(ExplicitTarget.transform.position - transform.position).normalized, settings.MaxRadiansOnAutoTarget * Time.deltaTime, 
				0f);
			rgb.velocity = targetVelocity;
			//Vector3
			//transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.identity, 10f * Time.deltaTime);
		}
	}

	private void UpdateActiveMaterial()
	{
		if (_explicitTarget == null && !_explosive && _bounces == 0f) renderer.material = settings.StandardProjectileMaterial;

		else if (_explicitTarget != null)
		{
			if (_explosive && _bounces != 0f) renderer.material = settings.StandardProjectileMaterial; // COMBINED MAT HERE
			else if (_explosive) renderer.material = settings.AutoAimAndExplosionCombinedMaterial;
			else if (_bounces != 0f) renderer.material = settings.AutoAimAndBounceCombinedMaterial;
			else renderer.material = settings.AutoAimCubeMaterial;
		}

		else if (_explosive)
		{
			if (_bounces != 0f) renderer.material = settings.BounceAndExplosionCombinedMaterial;
			else renderer.material = settings.ExplosionCubeMaterial;
		}

		else if (_bounces != 0f) renderer.material = settings.BounceCubeMaterial;
	}
}
