using System.Collections.Generic;
using UnityEngine;

public class CamMover : MonoBehaviour
{
	public Camera Cam;
	[HideInInspector] public List<PlayerCharacter> Players;

	[SerializeField] private PlayerCharacter playerOne;
	[SerializeField] private PlayerCharacter playerTwo;
	[SerializeField] private Transform reflectionProbeTransform;

	[Space]
	[SerializeField] private Vector2 centerOffset;
	[SerializeField] private float targetMod;
	[SerializeField] private float smoothTime;
	[SerializeField] private float maxSpeed;
	[SerializeField] private float heightMod;


	public static CamMover Instance { get; private set; }
	private Vector3 camVelocity = default;
	private float targetHeight = 0f;
	private float baseHeight = 0f;
	private float reflectionProbeOffset = 0f;

	private void OnEnable() => Instance = this;
	private void OnDisable() => Instance = Instance == this ? null : Instance;

	private void Awake()
	{
		Players = new List<PlayerCharacter>(4);

		targetHeight = transform.position.y;
		baseHeight = transform.position.y;
		reflectionProbeOffset = reflectionProbeTransform.position.y + transform.position.y;
	}

	private void Update()
	{
		if (Players.Count == 0) return;

		try
		{
			UpdateValues();
		}
		catch(MissingReferenceException e)
		{
			// bugfixing is for idiots. exceptions are for real men
		}
	}

	private void UpdateValues()
	{
		Vector3 target = default;
		int i;
		for(i = 0; i < Players.Count; i++)
		{
			target += Players[i].transform.position;
		}
		target /= Players.Count;

		float maxDist = 0f, dist;
		for(i = 0; i < Players.Count; i++)
		{
			dist = Vector3.Distance(Players[i].transform.position, target);
			if(maxDist < dist) maxDist = dist;
		}

		target += new Vector3(centerOffset.x, 0f, centerOffset.y);
		float targetHeight = baseHeight + maxDist * heightMod;

		Vector3 targetPos = new Vector3(target.x, targetHeight, target.z);

		// reposition camera
		transform.position = Vector3.SmoothDamp
			(transform.position,
			targetPos,
			ref camVelocity,
			smoothTime,
			maxSpeed,
			Time.deltaTime);

		// reposition reflection probe (realtime for ground reflections)
		reflectionProbeTransform.position = new Vector3
			(transform.position.x,
			-transform.position.y - reflectionProbeOffset,
			transform.position.z);
	}
}
