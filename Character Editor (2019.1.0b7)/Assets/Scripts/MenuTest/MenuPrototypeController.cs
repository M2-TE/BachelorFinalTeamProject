using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPrototypeController : MonoBehaviour
{
    [SerializeField] private GameObject gameStartButtons;
    [SerializeField] private GameObject onlineGameButtons;

    private void Start()
    {
        gameStartButtons.SetActive(false);
        onlineGameButtons.SetActive(false);
    }

    public void SelectStartGame()
    {
        onlineGameButtons.SetActive(false);
        gameStartButtons.SetActive(true);
    }

    public void SelectOptions()
    {

    }

    public void SelectBuildingTool()
    {

    }

    public void SelectOnlineGame()
    {
        onlineGameButtons.SetActive(true);
    }

    public void SelectExitGame()
    {
        Application.Quit();
    }
}
