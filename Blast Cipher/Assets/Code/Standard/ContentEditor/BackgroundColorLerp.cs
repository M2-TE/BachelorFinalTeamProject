using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BackgroundColorLerp : MonoBehaviour
{
    [SerializeField] private float duration;
    [SerializeField] private Color[] ColorCheckpoints  = new Color[1];
    [SerializeField] private bool onlineMode;
    [SerializeField] private Color OfflineColor = Color.gray;

    private float time = 0f;
    private int checkpoint = 0;
    private int nextCheck = 0;
    private Camera cam;

    private Color currentColor;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (onlineMode)
        {
            cam.backgroundColor = Color.Lerp(time < 0 ? OfflineColor : ColorCheckpoints[checkpoint], time < 0 ? ColorCheckpoints[checkpoint] : ColorCheckpoints[nextCheck], time < 0 ? 1 + time : time);
            if (time < 1)
                time += Time.deltaTime/duration;
            else
            {
                time = 0f;
                UpdateCheckpoints();
            }
        }
        else
        {
            if (time > -1)
            {
                cam.backgroundColor = Color.Lerp(currentColor, OfflineColor, Mathf.Abs(time));
                time -= Time.deltaTime / duration;
            }
        }
    }

    private void UpdateCheckpoints()
    {
        checkpoint = nextCheck;
        nextCheck = nextCheck + 1 >= ColorCheckpoints.Length ? 0 : nextCheck + 1;
    }

    public void ChangeMode()
    {
        onlineMode = !onlineMode;
        currentColor = cam.backgroundColor;
        time = onlineMode ? -1f : 0f;
    }
}
