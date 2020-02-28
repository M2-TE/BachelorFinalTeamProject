using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;

public enum ProfileSelectionState { Name, Settings, Achievements, Switch, Save, New }
public enum NameSelectionState { Descriptor, FirstLetter, SecondLetter, ThirdLetter }

public class ProfileSelectionManager : MenuManager
{
    [SerializeField] private MaterialsHolder username, settings, achievements, switchProfile, save, newProfile, letterSelection1, letterSelection2, letterSelection3;
    [SerializeField] private ProfileSelectionState standartState = 0;
    [SerializeField] private NameSelectionState standartLetterState = 0;
    [SerializeField] private Transform[] selectorPoints;
    [SerializeField] private Transform selector;

    private float selectorSpeed = 10f;
    
    private ProfileSelectionState currentState;
    private NameSelectionState currentLetterState;

    public ProfileSelectionState CurrentState { get => currentState; private set => SetMaterials(currentState,currentState = value); }
    public NameSelectionState CurrentLetterState { get => currentLetterState; private set => SetMaterials(currentLetterState,currentLetterState = value); }

    public void ChangeState(bool increment, bool sideways)
    {
        if (increment)
        {
            if (sideways && currentState == ProfileSelectionState.Name)
            {
                CurrentLetterState = ((int)CurrentLetterState) + 1 >= System.Enum.GetValues(typeof(NameSelectionState)).Length ? CurrentLetterState : CurrentLetterState + 1;
            }
            else if(currentState != ProfileSelectionState.Name || (currentState == ProfileSelectionState.Name && currentLetterState == NameSelectionState.Descriptor) )
            {
                CurrentState = ((int)CurrentState) + 1 >= System.Enum.GetValues(typeof(ProfileSelectionState)).Length ? CurrentState : CurrentState + 1;
                StartCoroutine(SelectorPositionChange());
            }
            else
            {
                // ToDo: Hier muss noch der Buchstaben wechsel rein
            }
        }
        else
        {
            if (sideways && currentState == ProfileSelectionState.Name)
            {
                CurrentLetterState = ((int)CurrentLetterState) -1 < 0 ? CurrentLetterState : CurrentLetterState - 1;
            }
            else if (currentState != ProfileSelectionState.Name || (currentState == ProfileSelectionState.Name && currentLetterState == NameSelectionState.Descriptor))
            {
                CurrentState = ((int)CurrentState) - 1 < 0 ? CurrentState : CurrentState - 1;
                StartCoroutine(SelectorPositionChange());
            }
            else
            {
                // ToDo: Hier muss noch der Buchstaben wechsel rein
            }
        }
    }

    private IEnumerator SelectorPositionChange()
    {
        Vector3 Start = selector.position;
        float startTime = Time.time;
        float lenght = Vector3.Distance(Start, selectorPoints[(int)CurrentState].position);

        while (selector.position != selectorPoints[(int)CurrentState].position)
        {
            float distance = (Time.time - startTime) * selectorSpeed * 10;
            selector.position = Vector3.Lerp(Start, selectorPoints[(int)CurrentState].position, distance / lenght);
            yield return new WaitForEndOfFrame();
        }
    }

    private void SetMaterials(ProfileSelectionState defaultMatState, ProfileSelectionState highlightedMatState)
    {
        SetMaterial(defaultMatState, defaultMat);
        SetMaterial(highlightedMatState, highlightedMat);
    }

    private void SetMaterials(NameSelectionState defaultMatState, NameSelectionState highlightedMatState)
    {
        SetMaterial(defaultMatState, defaultMat);
        SetMaterial(highlightedMatState, highlightedMat);
    }

    private void SetMaterial(ProfileSelectionState state, Material mat)
    {
        switch (state)
        {
            case ProfileSelectionState.Name:
                username.SetMaterials(mat);
                break;
            case ProfileSelectionState.Settings:
                settings.SetMaterials(mat);
                break;
            case ProfileSelectionState.Achievements:
                achievements.SetMaterials(mat);
                break;
            case ProfileSelectionState.Switch:
                switchProfile.SetMaterials(mat);
                break;
            case ProfileSelectionState.Save:
                save.SetMaterials(mat);
                break;
            case ProfileSelectionState.New:
                newProfile.SetMaterials(mat);
                break;
            default:
                break;
        }
    }

    private void SetMaterial(NameSelectionState state, Material mat)
    {
        switch (state)
        {
            case NameSelectionState.Descriptor:
                username.SetMaterials(mat);
                break;
            case NameSelectionState.FirstLetter:
                letterSelection1.SetMaterials(mat);
                break;
            case NameSelectionState.SecondLetter:
                letterSelection2.SetMaterials(mat);
                break;
            case NameSelectionState.ThirdLetter:
                letterSelection3.SetMaterials(mat);
                break;
            default:
                break;
        }
    }

    private void Start()
    {
        CurrentState = standartState;
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
