using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MenuState { Local, Online, CharacterEditor, Settings, Exit }

[RequireComponent(typeof(PressStartBlinker))]
public class MenuSelectionManager : MonoBehaviour
{
    [SerializeField] private MaterialsHolder localGame, onlineGame, characterEditor, settings, exit;
    [SerializeField] private MenuState standartState = 0;
    [SerializeField] private Material defaultMat, highlightedMat;
    [SerializeField] private Transform[] selectorPoints;
    [SerializeField] private Transform selector;
    [SerializeField][Range(10f,20f)] private float selectorSpeed = 10f;
    [SerializeField] private Transform mainCamera;
    [SerializeField] [Range(10f, 50f)] private float cameraSpeed = 1f;

    private bool isFocused = false;
    private MenuState currentState;

    public MenuState CurrentState { get => currentState; private set => SetMaterials(currentState,currentState = value); }

    public bool Focus { get => isFocused; set => isFocused = value; }

    public void ChangeState(bool increment)
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
        float start = toMenu ? 0 : 90;
        float finish = toMenu ? 90 : 0;
        float startTime = Time.time;
        while (mainCamera.rotation != Quaternion.Euler(finish, 0, 0))
        {
            float distance = (Time.time - startTime) * cameraSpeed * 10;
            mainCamera.rotation = Quaternion.Lerp(Quaternion.Euler(start,0,0),Quaternion.Euler(finish,0,0), distance / 90);
            yield return new WaitForEndOfFrame();
        }
        Focus = toMenu;
    }

    private void SetMaterials(MenuState defaultMatState, MenuState highlightedMatState)
    {
        SetMaterial(defaultMatState, defaultMat);
        SetMaterial(highlightedMatState, highlightedMat);
    }

    private void SetMaterial(MenuState state, Material mat)
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
            case MenuState.Settings:
                settings.SetMaterials(mat);
                break;
            case MenuState.Exit:
                exit.SetMaterials(mat);
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
        if (Input.GetKeyDown(KeyCode.Return) && !Focus)
            StartCoroutine(RotateCamera(true));
        else if (Input.GetKeyDown(KeyCode.Escape) && Focus)
            StartCoroutine(RotateCamera(false));
        else if(Focus == true)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                ChangeState(true);
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                ChangeState(false);
        }
    }
}
