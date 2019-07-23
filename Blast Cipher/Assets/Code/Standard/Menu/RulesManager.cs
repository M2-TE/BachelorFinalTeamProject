using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;

public enum RulesState { Rounds, Start }

public class RulesManager : MenuManager
{
    [SerializeField] MaterialsHolder rounds, start;
    [SerializeField] RulesState standartState = 0;
    [SerializeField] LocalLobbyManager localLobbyManager;
    [SerializeField] Transform[] singleDigits;
    [SerializeField] Transform[] doubleDigits;
    [SerializeField] private int roundAmount = 15;

    public float RulesAppearSpeed = 10f;

    private RulesState currentState;

    public RulesState CurrentState { get => currentState; private set => SetMaterials(currentState, currentState = value); }

    private void ChangeState(bool increment)
    {
        if (increment)
            CurrentState = ((int)CurrentState) + 1 >= System.Enum.GetValues(typeof(MenuState)).Length ? CurrentState : CurrentState + 1;
        else
            CurrentState = ((int)CurrentState) - 1 < 0 ? CurrentState : CurrentState - 1;
    }

    private void ChangeRoundAmount(bool increment)
    {
        if (increment)
        {
            roundAmount++;
            if (roundAmount >= 100)
                roundAmount = 1;
        }
        else
        {
            roundAmount--;
            if (roundAmount <= 0)
                roundAmount = 99;
        }

        int singleDigit = roundAmount % 10;
        int doubleDigit = Mathf.FloorToInt(roundAmount / 10);

        for (int i = 0; i < 10; i++)
        {
            if (i != singleDigit)
                singleDigits[i].gameObject.SetActive(false);
            else
                singleDigits[i].gameObject.SetActive(true);
        }
        for (int i = 0; i < 10; i++)
        {
            if (i != doubleDigit)
                doubleDigits[i].gameObject.SetActive(false);
            else
                doubleDigits[i].gameObject.SetActive(true);
        }
    }

    private void SetMaterials(RulesState defaultMatState, RulesState highlightedMatState)
    {
        SetMaterials(defaultMatState, defaultMat);
        SetMaterials(highlightedMatState, highlightedMat);
    }

    private void SetMaterials(RulesState state, Material mat)
    {
        switch (state)
        {
            case RulesState.Rounds:
                rounds.SetMaterials(mat);
                break;
            case RulesState.Start:
                start.SetMaterials(mat);
                break;
            default:
                break;
        }
    }

    private void ManageConfirmation()
    {
        switch (currentState)
        {
            case RulesState.Rounds:
                break;
            case RulesState.Start:
                GameManager.Instance.maxRounds = roundAmount;
                StartLocalGame();
                break;
        }
    }

    private void Start()
    {
        CurrentState = standartState;
        ChangeRoundAmount(true);
        ChangeRoundAmount(false);
    }

    private void StartLocalGame()
    {
        GameManager.Instance.LoadScene(3);
    }

    public override void OnConfirmation(InputAction.CallbackContext ctx)
    {
        ManageConfirmation();
    }

    public override void OnDecline(InputAction.CallbackContext ctx)
    {
        localLobbyManager.Rules = false;
    }

    public override void OnDPadInput(InputAction.CallbackContext ctx)
    {
        Vector2 inputValue = ctx.ReadValue<Vector2>();
        if (inputValue.y > 0f)
            ChangeState(false);
        else if (inputValue.y < 0f)
            ChangeState(true);

        if (currentState.Equals(RulesState.Rounds))
        {
            if (inputValue.x > 0f)
                ChangeRoundAmount(true);
            else if (inputValue.x < 0f)
                ChangeRoundAmount(false);
        }
    }

    public override void OnStartPressed(InputAction.CallbackContext ctx)
    {
    }
}
