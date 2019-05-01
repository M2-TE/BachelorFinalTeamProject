using System.Collections;
using UnityEngine;

public class Hazard : MonoBehaviour
{
	[SerializeField] private float DespawnTimer = .5f;

	private IEnumerator Start()
	{
		yield return new WaitForSeconds(DespawnTimer);
		Destroy(transform.parent.gameObject);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player")) other.GetComponent<PlayerCharacter>().TriggerDeath();
	}
}
