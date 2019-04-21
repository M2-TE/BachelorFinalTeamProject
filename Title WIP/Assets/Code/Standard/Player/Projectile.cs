﻿using UnityEngine;

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

	public Vector3 actualVelocity;
	public Rigidbody rgb;
	public new Collider collider;

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
			}
		}
	}
}
