using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Input;

public enum MenuState { Local, Settings , Editor, Profile, Exit }

[RequireComponent(typeof(PressStartBlinker))]
public class MenuSelectionManager : MenuManager
{
    [SerializeField] private MaterialsHolder localGame, settings, editors, profile, exit;
    [SerializeField] private readonly MenuState standartState = 0;
    [SerializeField] private readonly Transform[] selectorPoints;
    [SerializeField] private Transform selector;
    [SerializeField] [Range(10f,20f)] private float selectorSpeed = 10f;
    [SerializeField] private Transform mainCamera;
    [SerializeField] [Range(10f, 50f)] private float cameraSpeed = 1f;
    [SerializeField] [Range(0.5f, 0.1f)] private float buttonDelayAmount;
    [SerializeField] private LocalLobbyManager localLobbyManager;
    [SerializeField] private RulesManager rulesManager;
    [SerializeField] private ProfileSelectionManager profileSelectionManager;

    private MenuState currentState;
    private bool inTitleScreen;
    private MenuManager currentActiveManager;

    private float buttonDelay = 0f;

    public MenuState CurrentState { get => currentState; private set => SetMaterials(currentState,currentState = value); }

    private void ChangeState(bool increment)
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
        float length = Vector3.Distance(Start, selectorPoints[(int)CurrentState].position);

        while(selector.position != selectorPoints[(int)CurrentState].position)
        {
            float distance = (Time.time - startTime) * selectorSpeed * 10;
            selector.position = Vector3.Lerp(Start, selectorPoints[(int)CurrentState].position,distance / length);
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator RotateCamera(bool toTitleScreen)
    {
        float start = !toTitleScreen ? 0f : 90f;
        float finish = !toTitleScreen ? 90f : 0f;
        float startTime = Time.time;
        while (mainCamera.rotation != Quaternion.Euler(finish, 0, 0))
        {
            float distance = (Time.time - startTime) * cameraSpeed * 10f;
            mainCamera.rotation = Quaternion.Lerp(Quaternion.Euler(start,0,0),Quaternion.Euler(finish,0,0), distance / 90);
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator CameraSubmenuMovement(bool inSubmenu)
    {
        Vector3 start = !inSubmenu ? new Vector3(0f,30f,-10f) : new Vector3(0f,-10f,-10f);
        Vector3 finish = !inSubmenu ? new Vector3(0f,-10f,-10f) : new Vector3(0f,30f,-10f);
        float startTime = Time.time;
        while (mainCamera.position != finish)
        {
            float distance = (Time.time - startTime) * cameraSpeed * 10f;
            mainCamera.position = Vector3.Lerp(start, finish, distance / 40);
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator ManageRules(bool open)
    {
        Vector3 start = open? new Vector3(0, -50, 0) : new Vector3(0, -20, 0);
        Vector3 finish = open ? new Vector3(0, -20, 0) : new Vector3(0, -50, 0);
        float startTime = Time.time;
        float length = Vector3.Distance(start, finish);
        while (rulesManager.transform.position != finish)
        {
            float distance = (Time.time - startTime) * rulesManager.RulesAppearSpeed * 10;
            rulesManager.transform.position = Vector3.Lerp(start, finish, distance / length);
            yield return new WaitForEndOfFrame();
        }
    }

    protected void SetMaterials(MenuState defaultMatState, MenuState highlightedMatState)
    {
        SetMaterial(defaultMatState, defaultMat);
        SetMaterial(highlightedMatState, highlightedMat);
    }

    protected void SetMaterial(MenuState state, Material mat)
    {
        switch (state)
        {
            case MenuState.Local:
                localGame.SetMaterials(mat);
                break;
            case MenuState.Settings:
                settings.SetMaterials(mat);
                break;
            case MenuState.Editor:
                editors.SetMaterials(mat);
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

    public void ManageSubmenu(bool open)
    {
        switch (currentState)
        {
            case MenuState.Local:
                if (localLobbyManager.Rules)
                {
                    StartCoroutine(ManageRules(open));
                    currentActiveManager = open ? (MenuManager)rulesManager : (MenuManager)localLobbyManager;
                    rulesManager.ToggleActivation(open);
                }
                else
                {
                    StartCoroutine(CameraSubmenuMovement(!open));
                    currentActiveManager = open ? (MenuManager)localLobbyManager : this;
                    localLobbyManager.ToggleActivation(open);
                }
                break;
            case MenuState.Settings:
                // Settings
                break;
            case MenuState.Editor:
                GameManager.Instance.LoadScene(2);
                break;
            case MenuState.Profile:
                StartCoroutine(CameraSubmenuMovement(!open));
                currentActiveManager = open ? (MenuManager)profileSelectionManager : this;
                profileSelectionManager.ToggleActivation(open);
                break;
            case MenuState.Exit:
                Application.Quit();
                break;
            default:
                break;
        }
    }

    public override void OnDPadInput(InputAction.CallbackContext ctx)
    {
        if (buttonDelay > 0)
            return;
        else
            buttonDelay = buttonDelayAmount;
        if (!currentActiveManager.Equals(this))
            currentActiveManager.OnDPadInput(ctx);
        else
        {
            Vector2 inputValue = ctx.ReadValue<Vector2>();
            if (inputValue.y > 0f)
                ChangeState(false);
            else if (inputValue.y < 0f)
                ChangeState(true);
        }
    }

    public override void OnStartPressed(InputAction.CallbackContext ctx)
    {
        if (buttonDelay > 0)
            return;
        else
            buttonDelay = buttonDelayAmount;
        if (!currentActiveManager.Equals(this))
            currentActiveManager.OnStartPressed(ctx);
        else if(inTitleScreen)
            StartCoroutine(RotateCamera(inTitleScreen = false));
    }

    public override void OnConfirmation(InputAction.CallbackContext ctx)
    {
        if (buttonDelay > 0)
            return;
        else
            buttonDelay = buttonDelayAmount;
        if (!currentActiveManager.Equals(this))
            currentActiveManager.OnConfirmation(ctx);
        else
        {
            if (inTitleScreen)
                StartCoroutine(RotateCamera(inTitleScreen = false));
            else
                ManageSubmenu(true);
        }
    }

    public override void OnDecline(InputAction.CallbackContext ctx)
    {
        if (buttonDelay > 0)
            return;
        else
            buttonDelay = buttonDelayAmount;
        if (!currentActiveManager.Equals(this))
            currentActiveManager.OnDecline(ctx);
        else
            StartCoroutine(RotateCamera(inTitleScreen = true));
    }

    private void Start()
    {
        mainManager = this;
        CurrentState = standartState;
        currentActiveManager = this;
        inTitleScreen = true;
        GameManager.Instance.CheckForStandardContent();
    }

    private void Update()
    {
        if(buttonDelay > 0)
            buttonDelay -= Time.deltaTime;
    }

    private void OnEnable() => RegisterActions();

    private void OnDisable() => UnregisterActions();

    private void RegisterActions()
    {
        input.General.Start.performed += OnStartPressed;
        input.General.DPadInput.performed += OnDPadInput;
        input.CEditor.SouthButton.performed += OnConfirmation;
        input.CEditor.EastButton.performed += OnDecline;
    }

    private void UnregisterActions()
    {
        input.General.Start.performed -= OnStartPressed;
        input.General.DPadInput.performed -= OnDPadInput;
		input.CEditor.SouthButton.performed -= OnConfirmation;
		input.CEditor.EastButton.performed -= OnDecline;
	}

}
