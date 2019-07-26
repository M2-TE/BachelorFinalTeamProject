using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BackgroundColorLerp : MonoBehaviour
{
    [SerializeField] private float duration;
    [SerializeField] private Color[] ColorCheckpoints  = new Color[1];

    private float time = 0f;
    private int checkpoint = 0;
    private int nextCheck = 0;
    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        cam.backgroundColor = Color.Lerp(ColorCheckpoints[checkpoint], ColorCheckpoints[nextCheck], time);
        if (time < duration)
            time += Time.deltaTime;
        else
        {
            time = 0f;
            UpdateCheckpoints();
        }
    }

    private void UpdateCheckpoints()
    {
        checkpoint = nextCheck;
        nextCheck = nextCheck + 1 >= ColorCheckpoints.Length ? 0 : nextCheck + 1;
    }

}
