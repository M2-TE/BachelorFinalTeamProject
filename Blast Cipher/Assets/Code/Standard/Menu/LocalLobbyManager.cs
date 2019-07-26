using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.SceneManagement;

public enum LocalLobbyState { Selection, Team, Ready }
public enum SelectorState {  Right, Back , Left, Front }

public class LocalLobbyManager : MenuManager
{
    [SerializeField] private int maxPlayers = 4;
    [SerializeField] [Range(10f, 50f)] private float selectionWheelSpeed = 30f;
    [SerializeField] private LocalLobbyState standartState = 0;
    [SerializeField] private MaterialsHolder[] selectors, ready, team;
    [SerializeField] private PressStartBlinker[] notJoinedBlinker;
    [SerializeField] private Transform[] toggleNodes, readyNode, selectionWheel, teamOne, teamTwo, teamThree, teamFour;
    [SerializeField] private MeshFilter[] selectionBodyFront, selectionBodyBack, selectionBodyLeft, selectionBodyRight;

    private LocalLobbyState[] currentState = new LocalLobbyState[] { LocalLobbyState.Selection, LocalLobbyState.Selection, LocalLobbyState.Selection, LocalLobbyState.Selection };

    private InputDevice[] players = new InputDevice[] { null, null, null, null };

    private SelectorState[] visibleSelection = new SelectorState[] { SelectorState.Right,SelectorState.Left,SelectorState.Right,SelectorState.Left};
    private int[] currentCharacter = new int[]{0,0,0,0}, currentTeam = new int[] { 0, 1, 2, 3 };
    private int maxCharacter = 0;

    private bool[] inRotation = new bool[] { false, false, false, false };
    private bool rules;

    private bool[] isReady = new bool[] { false,false,false,false};

    public SelectorState GetVisibleSelection(int playerID) => visibleSelection[playerID];

    public void SetVisibleSelection(int playerID, SelectorState state, int change)
    {
        visibleSelection[playerID] = state;
        SetCharacterSelection(visibleSelection[playerID], change, playerID);
    }

    public LocalLobbyState GetCurrentState(int playerID) => currentState[playerID];

    public void SetCurrentState(int playerID, LocalLobbyState state)
    {
        SetMaterials(currentState[playerID], currentState[playerID] = state, playerID);
    }

    public bool Rules { get => rules; set => rules = SetRule(value); }

    private bool SetRule(bool rule)
    {
        if (rule)
        {
            rules = rule;
            List<Mesh> playerMeshes = new List<Mesh>();
            List<int> playerColors = new List<int>();
            for (int i = 0; i < maxPlayers; i++)
            {
                if (players[i] != null)
                {
                    playerMeshes.Add(MeshGenerator.GenerateMeshFromScriptableObject(GameManager.Instance.ContentHolder.Characters[currentCharacter[i]]));
                    playerColors.Add(GameManager.Instance.ContentHolder.Characters[currentCharacter[i]].CharacterColor);
                }
                else
                {
                    playerMeshes.Add(null);
                    playerColors.Add(0);
                }
            }
            GameManager.Instance.AssignPlayerMeshes(playerMeshes.ToArray());
            GameManager.Instance.AssignPlayerColors(playerColors.ToArray());
            GameManager.Instance.AssignPlayerTeams(currentTeam);
        }
        else
        {
            GameManager.Instance.AssignPlayerMeshes(new Mesh[] { null, null, null, null });
            GameManager.Instance.AssignPlayerColors(new int[] { 0, 0, 0, 0 });
            GameManager.Instance.AssignPlayerTeams(new int[] { 0, 1, 2, 3 });
            for (int playerID = 0; playerID < maxPlayers; playerID++)
            {
                DisplayReady(playerID, false);
            }
        }
        mainManager.ManageSubmenu(rule);
        return rule;
    }

    private void AssignedPlayerToggle(bool gained, InputAction.CallbackContext ctx)
    {
        if (gained && !DeviceAlreadyRegistered(ctx.control.device))
        {
            for (int playerID = 0; playerID < maxPlayers; playerID++)
            {
                if (players[playerID] == null) {
                    players[playerID] = ctx.control.device;
                    GameManager.Instance.inputDevices[playerID] = players[playerID];
                    TogglePlayerJoinedTransform(playerID);
                    return;
                }
            }
        }
        else if(!gained && DeviceAlreadyRegistered(ctx.control.device))
        {
            for (int playerID = 0; playerID < players.Length; playerID++)
            {
                if(players[playerID] == ctx.control.device)
                {
                    if (isReady[playerID])
                        DisplayReady(playerID, false);
                    else
                    {
                        players[playerID] = null;
                        GameManager.Instance.inputDevices[playerID] = null;
                        TogglePlayerJoinedTransform(playerID);
                    }
                    return;
                }
            }
        }
        else if (!gained)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null)
                    return;
            }
            mainManager.ManageSubmenu(false);
        }
    }
    
    private bool DeviceAlreadyRegistered(InputDevice device)
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == device)
                return true;
        }
        return false;
    }

    private void TogglePlayerJoinedTransform(int playerID)
    {
        SetCurrentState(playerID,standartState);
        notJoinedBlinker[playerID].Enabled = toggleNodes[playerID].gameObject.activeInHierarchy;
        toggleNodes[playerID].gameObject.SetActive(!toggleNodes[playerID].gameObject.activeInHierarchy);
    }

    private void ChangeState(bool increment, int playerID)
    {
        if (isReady[playerID])
            return;
        if (increment)
        {
            SetCurrentState(playerID,((int)GetCurrentState(playerID)) + 1 >= System.Enum.GetValues(typeof(LocalLobbyState)).Length ? GetCurrentState(playerID) : GetCurrentState(playerID) + 1);
        }
        else
        {
            SetCurrentState(playerID, ((int)GetCurrentState(playerID)) - 1 < 0 ? GetCurrentState(playerID) : GetCurrentState(playerID) - 1);
        }
    }

    private void ChangeCharacter(bool increment, int playerID)
    {
        StartCoroutine(RotateSelectionWheel(increment, playerID));
            
        if (increment)
        {
            inRotation[playerID] = true;
            currentCharacter[playerID] = (currentCharacter[playerID] - 1 < 0) ? maxCharacter : currentCharacter[playerID] - 1;
            SetVisibleSelection(playerID, ((int)GetVisibleSelection(playerID)) - 1 < 0 ? GetVisibleSelection(playerID) + (System.Enum.GetValues(typeof(SelectorState)).Length - 1) : GetVisibleSelection(playerID) - 1,-1);
        }
        else
        {
            inRotation[playerID] = true;
            currentCharacter[playerID] = (currentCharacter[playerID] + 1 > maxCharacter) ? 0 : currentCharacter[playerID] + 1;
            SetVisibleSelection(playerID, ((int)GetVisibleSelection(playerID)) + 1 > System.Enum.GetValues(typeof(SelectorState)).Length - 1 ? GetVisibleSelection(playerID) - (System.Enum.GetValues(typeof(SelectorState)).Length - 1) : GetVisibleSelection(playerID) + 1, 1);
        }
    }

    private IEnumerator RotateSelectionWheel(bool increment, int playerID)
    {
        float start = selectionWheel[playerID].rotation.eulerAngles.z;
        float finish = increment ? (Mathf.RoundToInt(start/90) - 1f)*90f : (Mathf.RoundToInt(start / 90) + 1f) * 90f;
        float startTime = Time.time;
        while(selectionWheel[playerID].rotation != Quaternion.Euler(0f,0f,finish))
        {
            float distance = (Time.time - startTime) * selectionWheelSpeed * 10f;
            selectionWheel[playerID].rotation = Quaternion.Lerp(Quaternion.Euler(0f, 0f, start), Quaternion.Euler(0f, 0f, finish), distance / 90f);    
            yield return new WaitForEndOfFrame();
        }
        inRotation[playerID] = false;
    }

    private void ChangeTeam(bool increment, int playerID)
    {
        if (increment)
        {
            currentTeam[playerID] = currentTeam[playerID] + 1 >= maxPlayers ? 0 : currentTeam[playerID] + 1;
        }
        else
        {
            currentTeam[playerID] = currentTeam[playerID] - 1 < 0 ? maxPlayers-1 : currentTeam[playerID] -1; 
        }
        SetTeam(playerID, currentTeam[playerID]);
    }

    private void SetTeam(int playerID, int team)
    {
        teamOne[playerID].gameObject.SetActive(team == 0 ? true : false);
        teamTwo[playerID].gameObject.SetActive(team == 1 ? true : false); 
        teamThree[playerID].gameObject.SetActive(team == 2 ? true : false);
        teamFour[playerID].gameObject.SetActive(team == 3 ? true : false);
    }

    private void SetMaterials(LocalLobbyState defaultMatState, LocalLobbyState highlightedMatState, int playerID)
    {
        SetMaterial(defaultMatState, defaultMat,playerID);
        SetMaterial(highlightedMatState, highlightedMat, playerID);
    }

    private void SetMaterial(LocalLobbyState state, Material mat, int playerID)
    {
        switch (state)
        {
            case LocalLobbyState.Selection:
                selectors[playerID].SetMaterials(mat);
                break;
            case LocalLobbyState.Ready:
                ready[playerID].SetMaterials(mat);
                break;
            case LocalLobbyState.Team:
                team[playerID].SetMaterials(mat);
                break;
            default:
                break;
        }
    }

    private void ManageConfirmation(int playerID)
    {
        switch (currentState[playerID])
        {
            case LocalLobbyState.Selection:
                break;
            case LocalLobbyState.Ready:
                DisplayReady(playerID, !isReady[playerID]);
                break;
            case LocalLobbyState.Team:
                break;
            default:
                break;
        }
    }

    private void DisplayReady(int playerID, bool ready)
    {
        isReady[playerID] = ready;
        readyNode[playerID].gameObject.SetActive(ready);
        if (ready && CheckIfAllReady())
            Rules = true;
    }

    private bool CheckIfAllReady()
    {
        int playersConnected = 0;
        for (int playerID = 0; playerID < maxPlayers; playerID++)
        {
            if (players[playerID] != null)
                playersConnected++;
            if (!isReady[playerID] && players[playerID] != null)
                return false;
        }
        if (!GameManager.Instance.AllowOneControllerGameStart && playersConnected < 1)
            return false;
        return true;
    }

    private void SetCharacterSelection()
    {
        maxCharacter = GameManager.Instance.ContentHolder.Characters.Count-1;
        for (int playerID = 0; playerID < maxPlayers; playerID++)
        {
            SetCharacterMesh(playerID % 2 > 0 ? SelectorState.Left : SelectorState.Right, currentCharacter[playerID], playerID);
            SetCharacterMesh(playerID % 2 > 0 ? SelectorState.Back : SelectorState.Front, maxCharacter, playerID);
            SetCharacterMesh(playerID % 2 > 0 ? SelectorState.Front : SelectorState.Back, currentCharacter[playerID] + 1 > maxCharacter ? maxCharacter : currentCharacter[playerID] + 1, playerID);
            SetCharacterMesh(playerID % 2 > 0 ? SelectorState.Right : SelectorState.Left, currentCharacter[playerID] + 2 > maxCharacter ? maxCharacter : currentCharacter[playerID] + 2, playerID);
        }
    }

    private void SetCharacterSelection(SelectorState current, int change, int playerID)
    {
        int nextCharacter = (currentCharacter[playerID] + change > maxCharacter) ? 0 : (currentCharacter[playerID] + change < 0 ? maxCharacter : currentCharacter[playerID] + change);

        SelectorState NextState = (int)(current + change) > System.Enum.GetValues(typeof(SelectorState)).Length - 1 ? current - (System.Enum.GetValues(typeof(SelectorState)).Length - 1) : (int)(current + change) < 0 ? current + (System.Enum.GetValues(typeof(SelectorState)).Length - 1) : (current + change);

        SetCharacterMesh(NextState, nextCharacter, playerID);
    }

    private void SetCharacterMesh(SelectorState position, int characterPosition, int playerID)
    {
        CScriptableCharacter character = GameManager.Instance.ContentHolder.Characters[characterPosition];
        switch (position)
        {
            case SelectorState.Right:
                selectionBodyRight[playerID].mesh.Clear();
                selectionBodyRight[playerID].mesh = MeshGenerator.GenerateMeshFromScriptableObject(character);
                selectionBodyRight[playerID].gameObject.GetComponent<MeshRenderer>().material = GameManager.Instance.CharacterMaterials[character.CharacterColor];
                break;
            case SelectorState.Front:
                selectionBodyFront[playerID].mesh.Clear();
                selectionBodyFront[playerID].mesh = MeshGenerator.GenerateMeshFromScriptableObject(character);
                selectionBodyFront[playerID].gameObject.GetComponent<MeshRenderer>().material = GameManager.Instance.CharacterMaterials[character.CharacterColor];
                break;
            case SelectorState.Left:
                selectionBodyLeft[playerID].mesh.Clear();
                selectionBodyLeft[playerID].mesh = MeshGenerator.GenerateMeshFromScriptableObject(character);
                selectionBodyLeft[playerID].gameObject.GetComponent<MeshRenderer>().material = GameManager.Instance.CharacterMaterials[character.CharacterColor];
                break;
            case SelectorState.Back:
                selectionBodyBack[playerID].mesh.Clear();
                selectionBodyBack[playerID].mesh = MeshGenerator.GenerateMeshFromScriptableObject(character);
                selectionBodyBack[playerID].gameObject.GetComponent<MeshRenderer>().material = GameManager.Instance.CharacterMaterials[character.CharacterColor];
                break;
            default:
                break;
        }
    }

    private void Start()
    {
        for (int playerID = 0; playerID < maxPlayers; playerID++)
        {
            SetCurrentState(playerID, standartState);
            toggleNodes[playerID].gameObject.SetActive(false);
            DisplayReady(playerID, false);
            SetTeam(playerID, currentTeam[playerID]);
        }
        rules = false;
        ToggleActivation(false);
        SetCharacterSelection();
    }

    public override void OnDPadInput(InputAction.CallbackContext ctx)
    {
        Vector2 inputValue = ctx.ReadValue<Vector2>();
        for (int playerID = 0; playerID < maxPlayers; playerID++)
        {
            if (ctx.control.device == players[playerID])
            {
                if (inputValue.y > 0f)
                    ChangeState(false, playerID);
                else if (inputValue.y < 0f)
                    ChangeState(true, playerID);
                else if (inputValue.x > 0f && GetCurrentState(playerID).Equals(LocalLobbyState.Selection) && !inRotation[playerID])
                    ChangeCharacter(false, playerID);
                else if (inputValue.x < 0f && GetCurrentState(playerID).Equals(LocalLobbyState.Selection) && !inRotation[playerID])
                    ChangeCharacter(true, playerID);
                else if (inputValue.x > 0f && GetCurrentState(playerID).Equals(LocalLobbyState.Team))
                    ChangeTeam(true, playerID);
                else if (inputValue.x < 0f && GetCurrentState(playerID).Equals(LocalLobbyState.Team))
                    ChangeTeam(false, playerID);

                return;
            }
        }
    }

    public override void OnConfirmation(InputAction.CallbackContext ctx)
    {
        for (int playerID = 0; playerID < maxPlayers; playerID++)
        {
            if (ctx.control.device == players[playerID])
            {
                ManageConfirmation(playerID);
                return;
            }
        }
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
