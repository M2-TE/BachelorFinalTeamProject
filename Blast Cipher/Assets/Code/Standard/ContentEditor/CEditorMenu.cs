using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CEditorMenu : MonoBehaviour
{
    public TextMeshProUGUI[] MenuOptions;
    [Range(0.01f, 0.2f)]
    public float PulseStrength;
    [Range(1f,10f)]
    public float PulseSpeed;

    public Color StandartColor, HighlightedColor;

    private int menuPosition = 0;

    public int MenuPosition { get => menuPosition; set => ChangePosition(menuPosition, value); }

    public void SetActive(bool active)
    {
        if (active)
            MenuPosition = 0;
        gameObject.SetActive(active);
    }

    private float timePassed = 0;

    private void Update()
    {
        if (CEditorManager.Instance.EditorInput.LeftButton)
        {
            timePassed += Time.deltaTime * PulseSpeed;

            MenuOptions[menuPosition].rectTransform.localScale = new Vector3((PulseStrength * Mathf.Sin(timePassed)) + 1, (PulseStrength * Mathf.Sin(timePassed)) + 1);
        }
    }

    private void ChangePosition(int old, int current)
    {
        menuPosition = current;
        MenuOptions[old].rectTransform.localScale = Vector3.one;
        MenuOptions[old].color = StandartColor;
        MenuOptions[current].color = HighlightedColor;
    }

}
