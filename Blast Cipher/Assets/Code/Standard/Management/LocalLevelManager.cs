using UnityEngine;

public class LocalLevelManager : MonoBehaviour
{
	[SerializeField] private GameObject playerPrefab;
	[SerializeField] private GameObject playerCooldownsPrefab;
	[SerializeField] private Transform[] spawnPositions;
	[SerializeField] private PowerUpDispenser[] dispensers;

	private void Start()
	{
		var manager = GameManager.Instance;
		PlayerCharacter player;
		CooldownDisplay display;
		for(int i = 0; i < manager.inputDevices.Length; i++)
		{
			if(manager.inputDevices[i] != null)
			{
				player = Instantiate(playerPrefab, spawnPositions[i].position, Quaternion.identity).GetComponent<PlayerCharacter>();
				display = Instantiate(playerCooldownsPrefab, spawnPositions[i].position, Quaternion.Euler(90f, 0f, 0f)).GetComponent<CooldownDisplay>();

				display.player = player;
			}
		}

		for(int i = 0; i < dispensers.Length; i++)
		{
			switch (dispensers[i].currentOfferedPU.type)
			{
				default:
				case PowerUpType.Bomb:
					if (manager.matchSettings.EnabledPowerups[0])
					{
						dispensers[i].respawnCooldown = manager.matchSettings.Spawnrates[0];
					}
					else
					{
						Destroy(dispensers[i].gameObject);
						Destroy(dispensers[i].currentOfferedPU);
					}
					break;

				case PowerUpType.Bounce:
					if (manager.matchSettings.EnabledPowerups[1])
					{
						dispensers[i].respawnCooldown = manager.matchSettings.Spawnrates[1];
					}
					else
					{
						Destroy(dispensers[i].gameObject);
						Destroy(dispensers[i].currentOfferedPU);
					}
					break;

				case PowerUpType.AutoAim:
					if (manager.matchSettings.EnabledPowerups[2])
					{
						dispensers[i].respawnCooldown = manager.matchSettings.Spawnrates[2];
					}
					else
					{
						Destroy(dispensers[i].gameObject);
						Destroy(dispensers[i].currentOfferedPU);
					}
					break;
			}
		}
	}
}
