using UnityEngine;

public class CamMover : MonoBehaviour
{
	public Camera Cam;
	[SerializeField] private PlayerCharacter playerOne;
	[SerializeField] private PlayerCharacter playerTwo;

	[Space]
	[SerializeField] private Vector2 centerOffset;
	[SerializeField] private float targetMod;
	[SerializeField] private float smoothTime;
	[SerializeField] private float maxSpeed;
	
	[SerializeField] private float minHeight;

	public static CamMover Instance { get; private set; }
	private Vector3 camVelocity = default;

	private void OnEnable() => Instance = this;
	private void OnDisable() => Instance = Instance == this ? null : Instance;

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

		transform.position = Vector3.SmoothDamp
			(transform.position, 
			transform.position + new Vector3(target.x, 0f, target.y), 
			ref camVelocity,
			smoothTime,
			maxSpeed,
			Time.deltaTime);
	}
}
