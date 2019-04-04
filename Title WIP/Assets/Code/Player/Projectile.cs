using UnityEngine;

public class Projectile : MonoBehaviour
{
	public Rigidbody rgb;
	public new Collider collider;

	private void OnCollisionEnter(Collision collision)
	{
		Debug.Log(collision.collider.name);
	}
}
