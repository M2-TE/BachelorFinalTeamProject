using System;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
	[SerializeField] private Portal opposingPortal;
	[SerializeField] private Transform postTeleportPosition;
	[SerializeField] private float teleportCooldown = 1f;
	[SerializeField] private float postTeleportForce = 30f;

	private readonly List<ITeleportable> recentlyTeleportedObjects = new List<ITeleportable>();
	private readonly List<float> recentlyTeleportedObjectsCooldowns = new List<float>();

	private void Update()
	{
		int index = 0;
		while(index < recentlyTeleportedObjectsCooldowns.Count)
		{
			if (recentlyTeleportedObjectsCooldowns[index] > 0f) recentlyTeleportedObjectsCooldowns[index] -= Time.deltaTime;
			else
			{
				recentlyTeleportedObjects.RemoveAt(index);
				recentlyTeleportedObjectsCooldowns.RemoveAt(index);
			}

			index++;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		var teleBool = other.GetComponent<ITeleportable>();
		if (teleBool != null && teleBool.CanBeTeleported)
		{
			teleBool.CanBeTeleported = false;
			recentlyTeleportedObjects.Add(teleBool);
			recentlyTeleportedObjectsCooldowns.Add(teleportCooldown);

			opposingPortal.OnPortalEnter(this, other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		var teleBool = other.GetComponent<ITeleportable>();
		if(teleBool != null && !recentlyTeleportedObjects.Contains(teleBool))
		{
			teleBool.CanBeTeleported = true;
		}
	}

	internal void OnPortalEnter(Portal senderPortal, Collider collider)
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
				newVelocity.y = 0f;

				rgb.velocity = Vector3.zero;
				rgb.AddForce(newVelocity.normalized * postTeleportForce, ForceMode.Impulse);
				rgb.angularVelocity = Vector3.zero;

				//Debug.DrawRay(transform.position, originalVelocity, Color.green, 5f);
				//Debug.DrawRay(transform.position, newVelocity, Color.red, 5f);
				break;

			case "Player":

				break;
		}
	}
}
