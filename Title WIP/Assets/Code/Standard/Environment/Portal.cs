using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
	[SerializeField] private Portal opposingPortal;
	[SerializeField] private Transform postTeleportPosition;
	[SerializeField] private float postTeleportForce = 30f;
	[SerializeField] private bool autoTargetOtherPlayer;
	[SerializeField] private float autoTargetPostForce = 17f;

	private void OnTriggerEnter(Collider other)
	{
		var teleportable = other.GetComponent<ITeleportable>();
		if (teleportable != null && teleportable.CanBeTeleported)
		{
			teleportable.CanBeTeleported = false;
			StartCoroutine(ReenableTeleportation(teleportable));
			opposingPortal.OnPortalEnter(this, other);
		}
	}

	private IEnumerator ReenableTeleportation(ITeleportable teleportable)
	{
		var fixedUpdateWaiter = new WaitForFixedUpdate();
		yield return fixedUpdateWaiter; // during this physics update, the object entered and exited the first teleporter collider
		yield return fixedUpdateWaiter; // during this one, the object may have entered the teleporter collider of the opposing teleporter
		teleportable.CanBeTeleported = true;
	}

	internal void OnPortalEnter(Portal senderPortal, Collider collider)
	{
		Vector3 offset = senderPortal.transform.position - collider.transform.position;
		switch (collider.tag)
		{
			case "Projectile":
				var projectile = collider.GetComponent<Projectile>();
				projectile.CanPickup = false;

				if (autoTargetOtherPlayer)
				{
					projectile.ExplicitTarget = GameManager.Instance.RequestNearestPlayer(projectile.InitialShooter);
				}

				var rgb = projectile.rgb;
				rgb.MovePosition(postTeleportPosition.position);

				Vector3 originalVelocity = projectile.actualVelocity;
				Vector3 newVelocity = Vector3.Reflect(originalVelocity, transform.up);
				newVelocity.y = 0f; // keep vector in x,z and ignore y
									
				// rotate new velocity according to both portals' rotations
				float rotationDiff = senderPortal.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y;
				Quaternion correctiveRotation = Quaternion.Euler(0f, rotationDiff, 0f);
				newVelocity = correctiveRotation * newVelocity; 

				// set actual velocity
				projectile.actualVelocity = newVelocity;

				// apply force
				rgb.velocity = Vector3.zero;
				rgb.AddForce(newVelocity.normalized * (autoTargetOtherPlayer ? autoTargetPostForce : postTeleportForce), ForceMode.Impulse);
				rgb.angularVelocity = Vector3.zero;


				//Debug.DrawRay(transform.position, originalVelocity, Color.green, 5f);
				//Debug.DrawRay(transform.position, newVelocity, Color.red, 5f);
				break;

			case "Player":
				Debug.Log("Player Teleported");
				break;
		}
	}
}
