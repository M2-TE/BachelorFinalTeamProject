using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FpsUpdater : MonoBehaviour
{
	[SerializeField] private float timeBetweenUpdates;
	[SerializeField] private TextMeshProUGUI fpsText;

	private IEnumerator Start()
	{
		var waiter = new WaitForSecondsRealtime(timeBetweenUpdates);

		while (true)
		{
			fpsText.text = (int)(1f / Time.deltaTime) + " fps";
			yield return waiter;
		}
	}
}
