using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CooldownDisplay : MonoBehaviour
{
	[SerializeField] private Image dashCooldownImage;
	[SerializeField] private Image parryCooldownImage;
	[SerializeField] private PlayerCharacter player;
	[SerializeField] private float offset;
	[SerializeField] private float movingOffset;
	[SerializeField] private float smoothTime;
	[SerializeField] private float maxSpeed;

	private Vector3 currentVelocity;
	
    void Update()
    {
		if (player.gameObject.activeInHierarchy == true)
		{
			transform.position = Vector3.SmoothDamp
				(transform.position,
				player.transform.position
					+ new Vector3(0f, player.CharController.velocity.sqrMagnitude > 0f ? movingOffset : offset, 0f),
				ref currentVelocity,
				smoothTime, maxSpeed, Time.deltaTime);

			dashCooldownImage.fillAmount = (player.Settings.DashCooldown - player.CurrentDashCooldown) / player.Settings.DashCooldown * .5f;
			parryCooldownImage.fillAmount = (player.Settings.ParryCooldown - player.CurrentParryCooldown) / player.Settings.ParryCooldown * .5f;
		}
		else
		{
			dashCooldownImage.transform.parent.gameObject.SetActive(false);
		}
    }
}
