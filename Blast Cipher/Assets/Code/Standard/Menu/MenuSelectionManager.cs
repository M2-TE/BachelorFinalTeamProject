using System.Collections;
using UnityEngine;

public enum MenuState { Local, Online, CharacterEditor, Profile, Exit }

[RequireComponent(typeof(PressStartBlinker))]
public class MenuSelectionManager : MenuManager
{

    [SerializeField] private MaterialsHolder localGame, onlineGame, characterEditor, profile, exit;
    [SerializeField] private MenuState standartState = 0;
    [SerializeField] private Transform[] selectorPoints;
    [SerializeField] private Transform selector;
    [SerializeField][Range(10f,20f)] private float selectorSpeed = 10f;
    [SerializeField] private Transform mainCamera;
    [SerializeField] [Range(10f, 50f)] private float cameraSpeed = 1f;
    [SerializeField] private LocalLobbyManager localLobbyManager;
    [SerializeField] private ProfileSelectionManager profileSelectionManager;

    private bool isFocused = false;
    private bool inSubmenu = false;
    private MenuState currentState;

    public MenuState CurrentState { get => currentState; private set => SetMaterials(currentState,currentState = value); }

    public bool Focus { get => isFocused; set => isFocused = value; }

    public override bool Activated { get => true; set => throw new System.NotImplementedException(); }

    public override void ChangeState(bool increment)
    {
        if (increment)
            CurrentState = ((int)CurrentState) + 1 >= System.Enum.GetValues(typeof(MenuState)).Length ? CurrentState : CurrentState + 1;
        else
            CurrentState = ((int)CurrentState) - 1 < 0 ? CurrentState : CurrentState - 1;
        StartCoroutine(SelectorPositionChange());
    }

    private IEnumerator SelectorPositionChange()
    {
        Vector3 Start = selector.position;
        float startTime = Time.time;
        float lenght = Vector3.Distance(Start, selectorPoints[(int)CurrentState].position);

        while(selector.position != selectorPoints[(int)CurrentState].position)
        {
            float distance = (Time.time - startTime) * selectorSpeed * 10;
            selector.position = Vector3.Lerp(Start, selectorPoints[(int)CurrentState].position,distance / lenght);
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator RotateCamera(bool toMenu)
    {
        Focus = false;
        float start = toMenu ? 0f : 90f;
        float finish = toMenu ? 90f : 0f;
        float startTime = Time.time;
        while (mainCamera.rotation != Quaternion.Euler(finish, 0, 0))
        {
            float distance = (Time.time - startTime) * cameraSpeed * 10f;
            mainCamera.rotation = Quaternion.Lerp(Quaternion.Euler(start,0,0),Quaternion.Euler(finish,0,0), distance / 90);
            yield return new WaitForEndOfFrame();
        }
        Focus = toMenu;
    }

    private IEnumerator CameraSubmenuMovement()
    {
        inSubmenu = !inSubmenu;
        Vector3 start = inSubmenu ? new Vector3(0f,30f,-10f) : new Vector3(0f,-10f,-10f);
        Vector3 finish = inSubmenu ? new Vector3(0f,-10f,-10f) : new Vector3(0f,30f,-10f);
        float startTime = Time.time;
        while (mainCamera.position != finish)
        {
            float distance = (Time.time - startTime) * cameraSpeed * 10f;
            mainCamera.position = Vector3.Lerp(start, finish, distance / 40);
            yield return new WaitForEndOfFrame();
        }
    }

    protected override void SetMaterials<MenuState>(MenuState defaultMatState, MenuState highlightedMatState)
    {
        SetMaterial(defaultMatState, defaultMat);
        SetMaterial(highlightedMatState, highlightedMat);
    }

    protected override void SetMaterial<MenuState>(MenuState state, Material mat)
    {
        switch (state)
        {
            case MenuState.Local:
                localGame.SetMaterials(mat);
                break;
            case MenuState.Online:
                onlineGame.SetMaterials(mat);
                break;
            case MenuState.CharacterEditor:
                characterEditor.SetMaterials(mat);
                break;
            case MenuState.Profile:
                profile.SetMaterials(mat);
                break;
            case MenuState.Exit:
                exit.SetMaterials(mat);
                break;
            default:
                break;
        }
    }

    private void ManageSubmenu(bool open)
    {
        switch (currentState)
        {
            case MenuState.Local:
                StartCoroutine(CameraSubmenuMovement());
                localLobbyManager.Activated = open;
                break;
            case MenuState.Online:
                break;
            case MenuState.CharacterEditor:
                break;
            case MenuState.Profile:
                StartCoroutine(CameraSubmenuMovement());
                profileSelectionManager.Activated = open;
                break;
            case MenuState.Exit:
                Application.Quit();
                break;
            default:
                break;
        }
    }

    private void Start()
    {
        CurrentState = standartState;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !Focus && !inSubmenu)
            StartCoroutine(RotateCamera(true));
        else if (Focus && !inSubmenu)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                StartCoroutine(RotateCamera(false));
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                ChangeState(true);
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                ChangeState(false);
            else if (Input.GetKeyDown(KeyCode.Return))
                ManageSubmenu(true);
        }
        else if(Focus && inSubmenu)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                ManageSubmenu(false);
        }
    }
}
