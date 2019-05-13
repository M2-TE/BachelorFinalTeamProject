using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OnlineLobbyState {  Selection, Ready, Connectors, Start }
public enum ConnectorState {  Host, Join }
public enum NumbersPadVertical {  first, second, third, forth, submit }
public enum NumbersPadHorizontal { first, second, third }

public class OnlineLobbyManager : MonoBehaviour
{
    [SerializeField] private MaterialsHolder selectors, ready, host, join, start, zero, one, two, three, four, five, six, seven, eight, nine, dot, remove, cancle, submit;
    [SerializeField] OnlineLobbyState standartState = 0;
    [SerializeField] Material deafaultMat, highlightedMat;
    [SerializeField] private Transform ToggleNode, NumbersPadNode;

    private OnlineLobbyState currentLobbyState;
    private ConnectorState currentConnectionState;
    private NumbersPadVertical numPadRowState;
    private NumbersPadHorizontal numPadColumnState;

    private bool activated = false;

    public void ChangeState(bool increment, bool sideways)
    {

    }

    private void SetMaterials(OnlineLobbyState defaultMatState, OnlineLobbyState highlightedMatState)
    {

    }

    private void SetMaterial(OnlineLobbyState state, Material mat)
    {

    }

    private void ToggleActivation(bool setActive)
    {

    }
}
