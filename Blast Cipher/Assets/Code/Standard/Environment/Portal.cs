using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
	[SerializeField] private PortalSettings portalSettings;

	[Space]
	public Transform postTeleportPosition;
	[SerializeField] private Portal opposingPortal;

	[NonSerialized] public LineRenderer lineRenderer;

	private void Awake()
	{
		if(opposingPortal != null && lineRenderer == null) ConnectPortals(opposingPortal);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (opposingPortal == null) return;
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

	private void OnPortalEnter(Portal senderPortal, Collider collider)
	{
		Vector3 offset = senderPortal.transform.position - collider.transform.position;
		switch (collider.tag)
		{
			case "Projectile":
				var projectile = collider.GetComponent<Projectile>();
				projectile.CanPickup = false;

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
				rgb.AddForce(newVelocity.normalized * portalSettings.PostTeleportForce, ForceMode.Impulse);
				rgb.angularVelocity = Vector3.zero;

				OneShotAudioManager.PlayOneShotAudio(portalSettings.teleportationSounds, transform.position, 1f);

				Debug.DrawRay(transform.position, originalVelocity, Color.green, 5f);
				Debug.DrawRay(transform.position, newVelocity, Color.red, 5f);
				break;

			case "Player":
				Debug.Log("Player Teleported");
				break;
		}
	}
	
	public void ConnectPortals(Portal otherPortal)
	{
		opposingPortal = otherPortal;
		otherPortal.opposingPortal = this;

		// create line renderer connection
		if (lineRenderer == null)
		{
			lineRenderer = Instantiate(portalSettings.portalConnectorPrefab).GetComponent<LineRenderer>();
		}
		opposingPortal.lineRenderer = lineRenderer;

		lineRenderer.SetPosition(0, postTeleportPosition.position);
		lineRenderer.SetPosition(1, otherPortal.postTeleportPosition.position);
	}
}
