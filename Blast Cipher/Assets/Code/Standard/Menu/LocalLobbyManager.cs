using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LocalLobbyState { Selection, Ready, Start}

[RequireComponent(typeof(PressStartBlinker))]
public class LocalLobbyManager : MonoBehaviour
{
    [SerializeField] private MaterialsHolder lSelectors, lReady, rSelectors, rReady, start;
    [SerializeField] private LocalLobbyState standartState = 0;
    [SerializeField] private Material defaultMat, highlightedMat;
    [SerializeField] private Transform ToggleNode, SecondPlayerToggleNode;

    private LocalLobbyState currentLeftState;
    private LocalLobbyState currentRightState;

    private PressStartBlinker notJoinedBlinker;
    private bool playerTwoJoined = false;
    private bool activated = false;

    public LocalLobbyState CurrentLeftState { get => currentLeftState; private set => SetMaterials(currentLeftState,currentLeftState = value, true); }
    public LocalLobbyState CurrentRightState { get => currentRightState; private set => SetMaterials(currentRightState, currentRightState = value, false); }
    public bool Activated { get => activated; set => ToggleActivation(value); }

    public void ChangeState(bool increment, bool leftSide)
    {
        if (increment)
        {
            if (leftSide)
                CurrentLeftState = ((int)CurrentLeftState) + 1 >= System.Enum.GetValues(typeof(LocalLobbyState)).Length ? CurrentLeftState : CurrentLeftState + 1;
            else
                CurrentRightState = ((int)CurrentRightState) + 1 >= System.Enum.GetValues(typeof(LocalLobbyState)).Length - 1 ? CurrentRightState : CurrentRightState + 1;
        }
        else
        {
            if (leftSide)
                CurrentLeftState = ((int)CurrentLeftState) - 1 < 0 ? CurrentLeftState : CurrentLeftState - 1;
            else
                CurrentRightState = ((int)CurrentRightState) - 1 < 0 ? CurrentRightState : CurrentRightState - 1;
        }
    }

    private void SetMaterials(LocalLobbyState defaultMatState, LocalLobbyState highlightedMatState, bool leftSide)
    {
        SetMaterial(defaultMatState, defaultMat,leftSide);
        SetMaterial(highlightedMatState, highlightedMat, leftSide);
    }

    private void SetMaterial(LocalLobbyState state, Material mat, bool leftSide)
    {
        switch (state)
        {
            case LocalLobbyState.Selection:
                if (leftSide)
                    lSelectors.SetMaterials(mat);
                else
                    rSelectors.SetMaterials(mat);
                break;
            case LocalLobbyState.Ready:
                if (leftSide)
                    lReady.SetMaterials(mat);
                else
                    rReady.SetMaterials(mat);
                break;
            case LocalLobbyState.Start:
                start.SetMaterials(mat);
                break;
            default:
                break;
        }
    }

    private void ToggleActivation(bool setActive)
    {
        ToggleNode.gameObject.SetActive(setActive);
        activated = setActive;

        SecondPlayerToggle(false);
    }

    private void SecondPlayerToggle(bool joined)
    {
        playerTwoJoined = joined;
        SecondPlayerToggleNode.gameObject.SetActive(joined);
        notJoinedBlinker.Enabled = !joined;
    }

    private void ManageConfirmation()
    {
        switch (currentLeftState)
        {
            case LocalLobbyState.Selection:
                break;
            case LocalLobbyState.Ready:
                break;
            case LocalLobbyState.Start:
                StartLocalGame();
                break;
            default:
                break;
        }
    }

    private void StartLocalGame()
    {
		GameManager.Instance.LoadScene(1);
		//SceneManager.LoadScene(1, LoadSceneMode.Single);
	}

    private void Start()
    {
        CurrentLeftState = standartState;
        CurrentRightState = standartState;
        notJoinedBlinker = gameObject.GetComponent<PressStartBlinker>();
        Activated = false;
    }

    private void Update()
    {
        if (activated)
        {
            if (Input.GetKeyDown(KeyCode.W))
                ChangeState(false, true);
            else if (Input.GetKeyDown(KeyCode.S))
                ChangeState(true, true);
            if (playerTwoJoined)
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))
                    ChangeState(true, false);
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                    ChangeState(false, false);
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow))
                    SecondPlayerToggle(true);
            }

			if (Input.GetKeyDown(KeyCode.Return))
			{
				ManageConfirmation();
			}
        }
    }
}
