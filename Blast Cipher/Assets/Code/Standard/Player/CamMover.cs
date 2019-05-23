using UnityEngine;

public class CamMover : MonoBehaviour
{
	public Camera Cam;
	[SerializeField] private PlayerCharacter playerOne;
	[SerializeField] private PlayerCharacter playerTwo;
	[SerializeField] private Transform reflectionProbeTransform;

	[Space]
	[SerializeField] private Vector2 centerOffset;
	[SerializeField] private float targetMod;
	[SerializeField] private float smoothTime;
	[SerializeField] private float maxSpeed;
	[SerializeField] private float targetPlayerDistSqr;
	[SerializeField] private float heightAdjustmentMod;
	
	//[SerializeField] private float minHeight;

	public static CamMover Instance { get; private set; }
	private Vector3 camVelocity = default;
	private float targetHeight = 0f;
	private float baseHeight = 0f;
	private float reflectionProbeOffset = 0f;

	private void OnEnable() => Instance = this;
	private void OnDisable() => Instance = Instance == this ? null : Instance;

	private void Awake()
	{
		targetHeight = transform.position.y;
		baseHeight = transform.position.y;
		reflectionProbeOffset = reflectionProbeTransform.position.y + transform.position.y;
	}

	private void Update()
	{
		if (playerOne == null || playerTwo == null) return;

		UpdateStuff();
	}

	private void UpdateStuff()
	{
		Vector2 playerOneViewportPos = Cam.WorldToViewportPoint(playerOne.transform.position);
		Vector2 playerTwoViewportPos = Cam.WorldToViewportPoint(playerTwo.transform.position);
		Vector2 middle = playerTwoViewportPos + (playerOneViewportPos - playerTwoViewportPos) * .5f;

		Vector2 target = middle - new Vector2(.5f, .5f) + centerOffset;
		target *= targetMod;

		float distanceSqr = (playerOneViewportPos - playerTwoViewportPos).sqrMagnitude;
		targetHeight = baseHeight + (distanceSqr - targetPlayerDistSqr) * heightAdjustmentMod;

		Vector3 targetPos = new Vector3(transform.position.x + target.x, targetHeight, transform.position.z + target.y);

		transform.position = Vector3.SmoothDamp
			(transform.position, 
			targetPos,
			ref camVelocity,
			smoothTime,
			maxSpeed,
			Time.deltaTime);

		reflectionProbeTransform.position = new Vector3
			(transform.position.x, 
			-transform.position.y - reflectionProbeOffset, 
			transform.position.z);
	}
}
