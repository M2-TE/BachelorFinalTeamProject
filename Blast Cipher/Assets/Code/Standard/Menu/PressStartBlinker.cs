using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressStartBlinker: MonoBehaviour
{
    [SerializeField] private bool defaultState = false;
    [SerializeField][Range(1,10)] private int blinkSpeed = 5;
    [SerializeField] [Range(-.9f, .9f)] private float showLength = 0f;
    [SerializeField] Transform pressStart;

    private bool isEnabled = true;
    private float timePassed = 0f;

    public bool Enabled { get => isEnabled; set => SetDefaultState(isEnabled = value); }

    void Update()
    {
        timePassed += Time.deltaTime;
        if (isEnabled)
        {
            ApplySinWave(timePassed);
        }
    }

    private void ApplySinWave(float sinX)
    {
        bool temp = (showLength + Mathf.Sin(sinX*blinkSpeed)) > 0;

        pressStart.gameObject.SetActive(temp);
    }

    private void SetDefaultState(bool enabled)
    {
        pressStart.gameObject.SetActive(enabled);
    }
}
