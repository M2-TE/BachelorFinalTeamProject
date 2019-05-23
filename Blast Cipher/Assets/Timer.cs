using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class Timer : MonoBehaviour
{


	private void Start()
	{
		text = GetComponent<TextMeshProUGUI>();
	}

	private void Awake()
	{
		time = 0;
	}

	private float time = 0;

	private TextMeshProUGUI text;

    void Update()
    {
		time += Time.deltaTime;
		text.text = "Time: " + time;
    }
}
