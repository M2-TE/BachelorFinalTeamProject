using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CooldownDisplay : MonoBehaviour
{
	public PlayerCharacter player;
	[SerializeField] private Image dashCooldownImage;
	[SerializeField] private Image parryCooldownImage;
	[SerializeField] private float offset;
	[SerializeField] private float movingOffset;
	[SerializeField] private float smoothTime;
	[SerializeField] private float maxSpeed;

	private Vector3 currentVelocity;
	
    void Update()
    {
		if (player != null && player.gameObject.activeInHierarchy == true)
		{
			transform.position = Vector3.SmoothDamp
				(transform.position,
				player.transform.position
					+ new Vector3(0f, player.CharController.velocity.sqrMagnitude > 0f ? movingOffset : offset, 0f),
				ref currentVelocity,
				smoothTime, maxSpeed, Time.deltaTime);

			dashCooldownImage.fillAmount = (GameManager.Instance.matchSettings.DashCD - player.CurrentDashCooldown) / GameManager.Instance.matchSettings.DashCD * .5f;
			parryCooldownImage.fillAmount = (GameManager.Instance.matchSettings.ShieldCD - player.CurrentParryCooldown) / GameManager.Instance.matchSettings.ShieldCD * .5f;
		}
		else
		{
			//dashCooldownImage.transform.parent.gameObject.SetActive(false);
			dashCooldownImage.fillAmount = 0f;
			parryCooldownImage.fillAmount = 0f;
		}
    }
}
