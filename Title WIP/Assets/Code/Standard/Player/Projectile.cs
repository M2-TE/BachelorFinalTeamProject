using UnityEngine;

public class Projectile : MonoBehaviour
{
	[SerializeField] private string playerTag = "Player";
	[SerializeField] private string wallTag = "Wall";
	[SerializeField] private float velocityChangeOnWallHit;
	public bool canPickup;
	public Rigidbody rgb;
	public new Collider collider;

	private void OnCollisionEnter(Collision collision)
	{
		var go = collision.gameObject;
		if (!canPickup)
		{
			if (go.CompareTag(playerTag))
			{
				go.GetComponent<PlayerCharacter>().TriggerDeath();
				Destroy(gameObject);
			}
			else if (go.CompareTag(wallTag))
			{
				rgb.constraints = RigidbodyConstraints.None;
				rgb.velocity *= velocityChangeOnWallHit;
				canPickup = true;
			}
		}
	}
}
