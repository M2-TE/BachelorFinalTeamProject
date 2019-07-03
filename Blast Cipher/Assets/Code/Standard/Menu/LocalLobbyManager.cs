using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.SceneManagement;

public enum LocalLobbyState { Selection, Ready, Start}
public enum SelectorState {  Right, Back , Left, Front }

public class LocalLobbyManager : MenuManager
{
    [SerializeField] private MaterialsHolder lSelectors, lReady, rSelectors, rReady, start;
    [SerializeField] private LocalLobbyState standartState = 0;
    [SerializeField] private Transform toggleNode, firstPlayerToggleNode, secondPlayerToggleNode, leftReady, rightReady;
    [SerializeField] private PressStartBlinker notJoinedBlinkerLeft, notJoinedBlinkerRight;
    [SerializeField] private Transform leftSelectionWheel, rightSelectionWheel;
    [SerializeField] [Range(10f, 50f)] private float selectionWheelSpeed = 30f;
    [SerializeField] private MeshFilter selectionBodyRight, selectionBodyFront, selectionBodyLeft, selectionBodyBack, secondSelectionBodyRight, secondSelectionBodyFront, secondSelectionBodyLeft, secondSelectionBodyBack;

    private LocalLobbyState currentLeftState;
    private LocalLobbyState currentRightState;

    private InputDevice playerOne, playerTwo;

    private SelectorState visibleLeftSelection;
    private SelectorState visibleRightSelection;
    private int currentLeftCharacter = 0;
    private int currentRightCharacter = 0;
    private int maxCharacter = 0;

    private bool inRotationLeft = false;
    private bool inRotationRight = false;

    private bool rules;

    private bool readyOne = false, readyTwo = false;

    public SelectorState VisibleLeftSelection { get => visibleLeftSelection; private set => SetCharacterSelection(visibleLeftSelection, visibleLeftSelection = value, true); }
    public SelectorState VisibleRightSelection { get => visibleRightSelection; private set => SetCharacterSelection(visibleRightSelection, visibleRightSelection = value, false); }

    public LocalLobbyState CurrentLeftState { get => currentLeftState; private set => SetMaterials(currentLeftState,currentLeftState = value, true); }
    public LocalLobbyState CurrentRightState { get => currentRightState; private set => SetMaterials(currentRightState, currentRightState = value, false); }

    public bool Rules { get => rules; set => rules = SetRule(value); }

    private bool SetRule(bool rule)
    {
        if (rule)
        {
            rules = rule;
            GameManager.Instance.AssignPlayerMeshes(MeshGenerator.GenerateMeshFromScriptableObject(GameManager.Instance.ContentHolder.Characters[currentLeftCharacter]), MeshGenerator.GenerateMeshFromScriptableObject(GameManager.Instance.ContentHolder.Characters[currentRightCharacter]));
        }
        else
            GameManager.Instance.AssignPlayerMeshes(null, null);
       mainManager.ManageSubmenu(rule);
        return rule;
    }

    private void AssignedPlayerToggle(bool gained, InputAction.CallbackContext ctx)
    {
        if (gained)
        {
            if(playerOne == null && playerTwo != ctx.control.device)
            {
                playerOne = ctx.control.device;
                GameManager.Instance.inputDevices[0] = ctx.control.device;
                TogglePlayerJoinedTransform(true);
            }
            else if(playerTwo == null && playerOne != ctx.control.device)
            {
                playerTwo = ctx.control.device;
                GameManager.Instance.inputDevices[1] = ctx.control.device;
                TogglePlayerJoinedTransform(false);
            }
        }
        else
        {
            if (playerTwo == ctx.control.device)
            {
                if(readyTwo)
                    DisplayReady(false, false);
                else
                {
                    playerTwo = null;
                    GameManager.Instance.inputDevices[0] = null;
                    TogglePlayerJoinedTransform(false);
                }
            }
            else if (playerOne == ctx.control.device)
            {
                if (readyOne)
                {
                    DisplayReady(true, false);
                }
                else
                {
                    playerOne = null;
                    GameManager.Instance.inputDevices[1] = null;
                    TogglePlayerJoinedTransform(true);
                }
            }
            else if(playerOne == null && playerTwo == null)
                mainManager.ManageSubmenu(false);
        }
    }

    private void TogglePlayerJoinedTransform(bool leftPlayer)
    {
        if (leftPlayer)
        {
            CurrentLeftState = standartState;
            notJoinedBlinkerLeft.Enabled = firstPlayerToggleNode.gameObject.activeInHierarchy;
            firstPlayerToggleNode.gameObject.SetActive(!firstPlayerToggleNode.gameObject.activeInHierarchy);
        }
        else
        {
            CurrentRightState = standartState;
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
                CurrentLeftState = ((int)CurrentLeftState) - 1 < (readyOne ? 1 : 0) ? CurrentLeftState : CurrentLeftState - 1;
            else
                CurrentRightState = ((int)CurrentRightState) - 1 < (readyTwo ? 1 : 0) ? CurrentRightState : CurrentRightState - 1;
        }
    }

    private void ChangeCharacter(bool increment, bool leftSide)
    {
        StartCoroutine(RotateSelectionWheel(increment, leftSide));

        if (increment)
        {
            if (leftSide)
            {
                inRotationLeft = true;
                currentLeftCharacter = (currentLeftCharacter - 1 < 0) ? maxCharacter : currentLeftCharacter - 1;
                VisibleLeftSelection = ((int)VisibleLeftSelection) - 1 < 0 ? VisibleLeftSelection + (System.Enum.GetValues(typeof(SelectorState)).Length - 1) : VisibleLeftSelection - 1;
            }
            else
            {
                inRotationRight = true;
                currentRightCharacter = (currentRightCharacter - 1 < 0) ? maxCharacter : currentRightCharacter - 1;
                VisibleRightSelection = ((int)VisibleRightSelection) - 1 < 0 ? VisibleRightSelection + (System.Enum.GetValues(typeof(SelectorState)).Length - 1) : VisibleRightSelection - 1;
            }
        }
        else
        {
            if (leftSide)
            {
                inRotationLeft = true;
                currentLeftCharacter = (currentLeftCharacter + 1 > maxCharacter) ? 0 : currentLeftCharacter + 1;
                VisibleLeftSelection = ((int)VisibleLeftSelection) + 1 > System.Enum.GetValues(typeof(SelectorState)).Length - 1 ? VisibleLeftSelection - (System.Enum.GetValues(typeof(SelectorState)).Length - 1) : VisibleLeftSelection + 1;              
            }
            else
            {
                inRotationRight = true;
                currentRightCharacter = (currentRightCharacter + 1 > maxCharacter) ? 0 : currentRightCharacter + 1;
                VisibleRightSelection = ((int)VisibleRightSelection) + 1 > System.Enum.GetValues(typeof(SelectorState)).Length - 1 ? VisibleRightSelection - (System.Enum.GetValues(typeof(SelectorState)).Length - 1) : VisibleRightSelection + 1;
            }
        }
    }

    private IEnumerator RotateSelectionWheel(bool increment, bool leftSide)
    {
        float start = leftSide ? leftSelectionWheel.rotation.eulerAngles.z : rightSelectionWheel.rotation.eulerAngles.z;
        float finish = increment ? (Mathf.RoundToInt(start/90) - 1f)*90f : (Mathf.RoundToInt(start / 90) + 1f) * 90f;
        float startTime = Time.time;
        while((leftSide ? leftSelectionWheel.rotation != Quaternion.Euler(0f,0f,finish) : rightSelectionWheel.rotation != Quaternion.Euler(0f, 0f, finish)))
        {
            float distance = (Time.time - startTime) * selectionWheelSpeed * 10f;
            if (leftSide)
                leftSelectionWheel.rotation = Quaternion.Lerp(Quaternion.Euler(0f, 0f, start), Quaternion.Euler(0f, 0f, finish), distance / 90f);
            else
                rightSelectionWheel.rotation = Quaternion.Lerp(Quaternion.Euler(0f, 0f, start), Quaternion.Euler(0f, 0f, finish), distance / 90f);
            yield return new WaitForEndOfFrame();
        }
        if (leftSide)
            inRotationLeft = false;
        else
            inRotationRight = false;
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
        if (leftSide)
            switch (currentLeftState)
            {
                case LocalLobbyState.Selection:
                    break;
                case LocalLobbyState.Ready:
                    DisplayReady(leftSide, !readyOne);
                    break;
                case LocalLobbyState.Start:
                    if (playerOne != null && playerTwo != null && readyOne && readyTwo)
                        Rules = true;
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
                    DisplayReady(leftSide, !readyTwo);
                    break;
                case LocalLobbyState.Start:
                    break;
                default:
                    break;
            }
    }

    private void DisplayReady(bool leftSide, bool ready)
    {
        if (leftSide)
        {
            readyOne = ready;
            leftReady.gameObject.SetActive(readyOne);
        }
        else
        {
            readyTwo = ready;
            rightReady.gameObject.SetActive(readyTwo);
        }
    }

    private void SetCharacterSelection()
    {
        maxCharacter = GameManager.Instance.ContentHolder.Characters.Count - 1;
        visibleLeftSelection = SelectorState.Right;
        SetCharacterMesh(SelectorState.Right, currentLeftCharacter, true);
        SetCharacterMesh(SelectorState.Front, maxCharacter,true);
        SetCharacterMesh(SelectorState.Back, currentLeftCharacter+1 > maxCharacter ? maxCharacter : currentLeftCharacter +1, true);
        SetCharacterMesh(SelectorState.Left, currentLeftCharacter + 2 > maxCharacter ? maxCharacter : currentLeftCharacter + 2, true);

        visibleRightSelection = SelectorState.Left;
        SetCharacterMesh(SelectorState.Left, currentRightCharacter, false);
        SetCharacterMesh(SelectorState.Back, maxCharacter, false);
        SetCharacterMesh(SelectorState.Front, currentRightCharacter + 1 > maxCharacter ? maxCharacter : currentRightCharacter + 1, false);
        SetCharacterMesh(SelectorState.Right, currentRightCharacter + 2 > maxCharacter ? maxCharacter : currentRightCharacter + 2, false);
    }

    private void SetCharacterSelection(SelectorState previous, SelectorState next, bool leftSide)
    { 
        int characterChange = ((int)(next-previous));
        characterChange = characterChange < -1 ? 1 : characterChange > 1 ? -1 : characterChange;
        int nextCharacter = 0;
        if (leftSide)
        {
            nextCharacter = (currentLeftCharacter + characterChange > maxCharacter) ? 0 : (currentLeftCharacter + characterChange < 0 ? maxCharacter: currentLeftCharacter + characterChange);
        }
        else
            nextCharacter = (currentRightCharacter + characterChange > maxCharacter) ? 0 : (currentRightCharacter + characterChange < 0 ? maxCharacter : currentRightCharacter + characterChange);

        SelectorState PreemptiveState = (int)(next + characterChange) > System.Enum.GetValues(typeof(SelectorState)).Length - 1 ? next - (System.Enum.GetValues(typeof(SelectorState)).Length - 1) : (int)(next + characterChange) < 0 ? next + (System.Enum.GetValues(typeof(SelectorState)).Length - 1) : (next + characterChange);


        Debug.Log("CharacterChanged to: " + nextCharacter + " At " + PreemptiveState.ToString() + " Wheeldirection: "+ characterChange);
        SetCharacterMesh(PreemptiveState, nextCharacter, leftSide);
    }

    private void SetCharacterMesh(SelectorState position, int characterPosition, bool playerOne)
    {
        CScriptableCharacter character = GameManager.Instance.ContentHolder.Characters[characterPosition];
        switch (position)
        {
            case SelectorState.Right:
                if (playerOne)
                {
                    selectionBodyRight.mesh.Clear();
                    selectionBodyRight.mesh = MeshGenerator.GenerateMeshFromScriptableObject(character);
                }
                else
                {
                    secondSelectionBodyRight.mesh.Clear();
                    secondSelectionBodyRight.mesh = MeshGenerator.GenerateMeshFromScriptableObject(character);
                }
                break;
            case SelectorState.Front:
                if (playerOne)
                {
                    selectionBodyFront.mesh.Clear();
                    selectionBodyFront.mesh = MeshGenerator.GenerateMeshFromScriptableObject(character);
                }
                else
                {
                    secondSelectionBodyFront.mesh.Clear();
                    secondSelectionBodyFront.mesh = MeshGenerator.GenerateMeshFromScriptableObject(character);
                }
                break;
            case SelectorState.Left:
                if (playerOne)
                {
                    selectionBodyLeft.mesh.Clear();
                    selectionBodyLeft.mesh = MeshGenerator.GenerateMeshFromScriptableObject(character);
                }
                else
                {
                    secondSelectionBodyLeft.mesh.Clear();
                    secondSelectionBodyLeft.mesh = MeshGenerator.GenerateMeshFromScriptableObject(character);
                }
                break;
            case SelectorState.Back:
                if (playerOne)
                {
                    selectionBodyBack.mesh.Clear();
                    selectionBodyBack.mesh = MeshGenerator.GenerateMeshFromScriptableObject(character);
                }
                else
                {
                    secondSelectionBodyBack.mesh.Clear();
                    secondSelectionBodyBack.mesh = MeshGenerator.GenerateMeshFromScriptableObject(character);
                }
                break;
            default:
                break;
        }
    }

    private void Start()
    {
        CurrentLeftState = standartState;
        CurrentRightState = standartState;
        firstPlayerToggleNode.gameObject.SetActive(false);
        secondPlayerToggleNode.gameObject.SetActive(false);
        rules = false;
        ToggleActivation(false);
        SetCharacterSelection();
        DisplayReady(true, false);
        DisplayReady(false, false);
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
            else if (inputValue.x > 0f && currentLeftState.Equals(LocalLobbyState.Selection) && !inRotationLeft)
                ChangeCharacter(false, true);
            else if (inputValue.x < 0f && currentLeftState.Equals(LocalLobbyState.Selection) && !inRotationLeft)
                ChangeCharacter(true, true);
        }
        else if (ctx.control.device == playerTwo)
        {
            if (inputValue.y > 0f)
                ChangeState(false, false);
            else if (inputValue.y < 0f)
                ChangeState(true, false);
            else if (inputValue.x > 0f && currentRightState.Equals(LocalLobbyState.Selection) && !inRotationRight)
                ChangeCharacter(false, false);
            else if (inputValue.x < 0f && currentRightState.Equals(LocalLobbyState.Selection) && !inRotationRight)
                ChangeCharacter(true, false);
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
