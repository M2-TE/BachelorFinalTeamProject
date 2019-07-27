using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CEditorManagerBootstrapper : Bootstrapper
{
    public InputMaster Input;
    public GameObject Frame;
    public float Dimension = 30f;
    [Range(0.1f, 0.5f)]
    public float ButtonDelayAmount;
    public CameraMovement CamMovement;
    public CEditorLoader LoadingMenu;
    public CEditorMenu EditorMenu;
    public BackgroundColorLerp Background;
    public TextMeshProUGUI CurrentWorkingPosition, LookDirection;

    public Transform mainCamera;

    public AudioClip[] AddCube, RemoveCube, Error, Swipe, Confirm, OpenMenu, CloseMenu, Save, SwipeError, ChangeColor;

    private CEditorManager cEditorManager;

    private void OnEnable() => cEditorManager.RegisterBootstrapper(this);
    private void OnDisable() => cEditorManager.UnregisterBootstrapper();

    private void Awake()
    {
        cEditorManager = CEditorManager.Instance;
    }

    private void Update() => cEditorManager.CEditorUpdate();
}
