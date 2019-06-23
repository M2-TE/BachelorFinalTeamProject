using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;

public enum OnlineLobbyState {  Selection, Ready, Connectors, Start }
public enum ConnectorState {  Host, Join }
public enum NumbersPadVertical {  first, second, third, forth, submit }
public enum NumbersPadHorizontal { first, second, third }

public class OnlineLobbyManager : MenuManager
{
    //[SerializeField] private MaterialsHolder selectors, ready, host, join, start, zero, one, two, three, four, five, six, seven, eight, nine, dot, remove, cancle, submit;
    [SerializeField] private MaterialsHolder selectors, ready, host, join, start;
    [SerializeField] OnlineLobbyState standartState = 0;

    private OnlineLobbyState currentLobbyState;
    private ConnectorState currentConnectorState;
    private NumbersPadVertical numPadRowState;
    private NumbersPadHorizontal numPadColumnState;

    public OnlineLobbyState CurrentLobbyState { get => currentLobbyState; private set => SetMaterials(currentLobbyState, currentLobbyState = value); }
    public ConnectorState CurrentConnectorState
    {
        get => currentConnectorState;
        private set
        {
            currentConnectorState = value;
            SetConnectorMaterials(highlightedMat);
        }
    }

    public void ChangeState(bool increment, bool sideways)
    {
        if (!sideways)
        {
            if (increment)
            {
                CurrentLobbyState = ((int)CurrentLobbyState) + 1 >= System.Enum.GetValues(typeof(NameSelectionState)).Length ? CurrentLobbyState : CurrentLobbyState + 1;
            }
            else
            {
                CurrentLobbyState = ((int)CurrentLobbyState) - 1 < 0 ? CurrentLobbyState : CurrentLobbyState - 1;
            }
        }
        else if (currentLobbyState.Equals(OnlineLobbyState.Connectors))
        {
            CurrentConnectorState = currentConnectorState.Equals(ConnectorState.Host) ? ConnectorState.Join : ConnectorState.Host;
        }
        else if (currentLobbyState.Equals(OnlineLobbyState.Selection))
        {
            // Hier muss die Selection des Chars rein
        }
    }

    private void SetMaterials(OnlineLobbyState defaultMatState, OnlineLobbyState highlightedMatState)
    {
        SetMaterial(defaultMatState, defaultMat);
        SetMaterial(highlightedMatState, highlightedMat);
    }

    private void SetMaterial(OnlineLobbyState state, Material mat)
    {
        switch (state)
        {
            case OnlineLobbyState.Selection:
                selectors.SetMaterials(mat);
                break;
            case OnlineLobbyState.Ready:
                ready.SetMaterials(mat);
                break;
            case OnlineLobbyState.Connectors:
                SetConnectorMaterials(mat);
                break;
            case OnlineLobbyState.Start:
                start.SetMaterials(mat);
                break;
        }
    }

    private void SetConnectorMaterials(Material mat)
    {
        if (currentConnectorState.Equals(ConnectorState.Host))
        {
            host.SetMaterials(mat);
            join.SetMaterials(defaultMat);
        }
        else
        {
            host.SetMaterials(defaultMat);
            join.SetMaterials(mat);
        }
    }

    private void Start()
    {
        CurrentLobbyState = standartState;
        ToggleActivation(false);
    }

    public override void OnDPadInput(InputAction.CallbackContext ctx)
    {
        Vector2 inputValue = ctx.ReadValue<Vector2>();

        if (inputValue.y > 0f)
            ChangeState(false, false);
        else if (inputValue.y < 0f)
            ChangeState(true, false);
        else if (inputValue.x > 0f)
            ChangeState(true, true);
        else if (inputValue.x < 0f)
            ChangeState(false, true);
    }

    public override void OnConfirmation(InputAction.CallbackContext ctx)
    {
        throw new System.NotImplementedException();
    }

    public override void OnDecline(InputAction.CallbackContext ctx)
    {
        mainManager.ManageSubmenu(false);
    }

    public override void OnStartPressed(InputAction.CallbackContext ctx)
    {
        
    }
}
