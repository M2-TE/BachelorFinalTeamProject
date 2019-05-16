using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.SceneManagement;

public enum LocalLobbyState { Selection, Ready, Start}

public class LocalLobbyManager : MenuManager
{
    [SerializeField] private MaterialsHolder lSelectors, lReady, rSelectors, rReady, start;
    [SerializeField] private LocalLobbyState standartState = 0;
    [SerializeField] private Transform toggleNode, firstPlayerToggleNode, secondPlayerToggleNode;
    [SerializeField] private PressStartBlinker notJoinedBlinkerLeft, notJoinedBlinkerRight;

    private LocalLobbyState currentLeftState;
    private LocalLobbyState currentRightState;

    private InputDevice playerOne, playerTwo;

    public LocalLobbyState CurrentLeftState { get => currentLeftState; private set => SetMaterials(currentLeftState,currentLeftState = value, true); }
    public LocalLobbyState CurrentRightState { get => currentRightState; private set => SetMaterials(currentRightState, currentRightState = value, false); }

    private void AssignedPlayerToggle(bool gained, InputAction.CallbackContext ctx)
    {
        if (gained)
        {
            if(playerOne == null && playerTwo != ctx.control.device)
            {
                playerOne = ctx.control.device;
                TogglePlayerJoinedTransform(true);
            }
            else if(playerTwo == null && playerOne != ctx.control.device)
            {
                playerTwo = ctx.control.device;
                TogglePlayerJoinedTransform(false);
            }
        }
        else
        {
            if (playerTwo == ctx.control.device)
            {
                playerTwo = null;
                TogglePlayerJoinedTransform(false);
            }
            else if (playerOne == ctx.control.device)
            {
                playerOne = null;
                TogglePlayerJoinedTransform(true);
            }
            else if(playerOne == null && playerTwo == null)
                mainManager.ManageSubmenu(false);
        }
    }

    private void TogglePlayerJoinedTransform(bool leftPlayer)
    {
        if (leftPlayer)
        {
            notJoinedBlinkerLeft.Enabled = firstPlayerToggleNode.gameObject.activeInHierarchy;
            firstPlayerToggleNode.gameObject.SetActive(!firstPlayerToggleNode.gameObject.activeInHierarchy);
        }
        else
        {
            notJoinedBlinkerRight.Enabled = secondPlayerToggleNode.gameObject.activeInHierarchy;
            secondPlayerToggleNode.gameObject.SetActive(!secondPlayerToggleNode.gameObject.activeInHierarchy);
        }
    }

    private void ChangeState(bool increment, bool leftSide)
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

    private void ManageConfirmation(bool leftSide)
    {
        if(leftSide)
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
        else
            switch (currentRightState)
            {
                case LocalLobbyState.Selection:
                    break;
                case LocalLobbyState.Ready:
                    break;
                case LocalLobbyState.Start:
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
        firstPlayerToggleNode.gameObject.SetActive(false);
        secondPlayerToggleNode.gameObject.SetActive(false);
    }

    public override void OnDPadInput(InputAction.CallbackContext ctx)
    {
        Vector2 inputValue = ctx.ReadValue<Vector2>();

        if (ctx.control.device == playerOne)
        {
            if (inputValue.y > 0f)
                ChangeState(false, true);
            else if (inputValue.y < 0f)
                ChangeState(true, true);

            // Sideways
        }
        else if (ctx.control.device == playerTwo)
        {
            if (inputValue.y > 0f)
                ChangeState(false, false);
            else if (inputValue.y < 0f)
                ChangeState(true, false);

            // Sideways
        }
    }

    public override void OnConfirmation(InputAction.CallbackContext ctx)
    {
        if (ctx.control.device == playerOne)
            ManageConfirmation(true);
        else if (ctx.control.device == playerTwo)
            ManageConfirmation(false);
    }

    public override void OnDecline(InputAction.CallbackContext ctx)
    {
        AssignedPlayerToggle(false,ctx); // already implemented back to menu function
    }

    public override void OnStartPressed(InputAction.CallbackContext ctx)
    {
        AssignedPlayerToggle(true,ctx); // already implemented controller overflow and duplicate ignore function
    }
}
