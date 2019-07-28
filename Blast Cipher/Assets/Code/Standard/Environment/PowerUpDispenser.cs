using UnityEngine;
using UnityEngine.UI;

public class PowerUpDispenser : MonoBehaviour
{
	[SerializeField] private GameObject powerUpPrefab;
	[SerializeField] private Image cooldownImage;
	public float respawnCooldown;

	[HideInInspector] public PowerUp currentOfferedPU;
	private float currentRespawnTimer;

	private void Awake()
	{
		currentOfferedPU = Instantiate(powerUpPrefab, transform).GetComponent<PowerUp>();
		currentRespawnTimer = 0f;
	}

	private void Update()
	{
		if(!currentOfferedPU.gameObject.activeInHierarchy)
		{
			if(currentRespawnTimer < respawnCooldown)
			{
				cooldownImage.fillAmount = currentRespawnTimer / respawnCooldown;
				currentRespawnTimer += Time.deltaTime;
			}
			else
			{
				currentOfferedPU.gameObject.SetActive(true);
				currentRespawnTimer = 0f;
			}
		}
		else
		{
			cooldownImage.fillAmount = 1f;
		}
	}
}
