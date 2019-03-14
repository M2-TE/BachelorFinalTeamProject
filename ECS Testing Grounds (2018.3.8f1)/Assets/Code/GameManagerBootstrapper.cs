using UnityEngine;

public class GameManagerBootstrapper : MonoBehaviour
{
	public GameObject EntityPrefab;

	private void Start()
	{
		GameManager.Instance.Register(this);
	}
}
