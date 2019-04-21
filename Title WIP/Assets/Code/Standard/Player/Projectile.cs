using UnityEngine;

public class Projectile : MonoBehaviour, ITeleportable
{
	[SerializeField] private string playerTag = "Player";
	[SerializeField] private string wallTag = "Wall";
	[SerializeField] private float velocityChangeOnWallHit;

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

	public Vector3 actualVelocity; // TODO update this value properly to handle portal physics
	public Rigidbody rgb;
	public new Collider collider;

	public PlayerCharacter InitialShooter;
	public PlayerCharacter ExplicitTarget;

	public bool CanBeTeleported { get; set; } = true;

	private void OnCollisionEnter(Collision collision)
	{
		var go = collision.gameObject;
		if (!CanPickup)
		{
			if (go.CompareTag(playerTag))
			{
				go.GetComponent<PlayerCharacter>().TriggerDeath();
				Destroy(gameObject);
			}
			else if (go.CompareTag(wallTag) && CanBeTeleported)
			{
				rgb.angularVelocity = Vector3.zero;
				rgb.velocity = rgb.velocity.normalized * velocityChangeOnWallHit;
				CanPickup = true;
				ExplicitTarget = null;
			}
		}
	}

	private void Update()
	{
		if(ExplicitTarget != null && !CanPickup)
		{
			Vector3 targetVelocity = Vector3.RotateTowards(rgb.velocity, (ExplicitTarget.transform.position - transform.position).normalized, 20f * Time.deltaTime, 0f);
			rgb.velocity = targetVelocity;
			//Vector3
			//transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.identity, 10f * Time.deltaTime);
		}
	}
}
