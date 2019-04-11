using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPrototypeController : MonoBehaviour
{
    [SerializeField] private GameObject gameStartButtons;
    [SerializeField] private GameObject onlineGameButtons;
    [SerializeField] private GameObject localGameButtons;

    [SerializeField] private GameObject optionsPanel;

    private void Start()
    {
        gameStartButtons.SetActive(false);
        onlineGameButtons.SetActive(false);
        localGameButtons.SetActive(false);
        optionsPanel.SetActive(false);
    }

    public void SelectStartGame()
    {
        optionsPanel.SetActive(false);
        onlineGameButtons.SetActive(false);
        localGameButtons.SetActive(false);
        gameStartButtons.SetActive(true);
    }

    public void SelectOptions()
    {
        optionsPanel.SetActive(true);
        onlineGameButtons.SetActive(false);
        localGameButtons.SetActive(false);
        gameStartButtons.SetActive(false);
    }

    public void SelectBuildingTool()
    {

    }

    public void SelectLocalGame()
    {
        onlineGameButtons.SetActive(false);
        localGameButtons.SetActive(true);
    }

    public void SelectOnlineGame()
    {
        onlineGameButtons.SetActive(true);
        localGameButtons.SetActive(false);
    }

    public void SelectExitGame()
    {
        Application.Quit();
    }
}
