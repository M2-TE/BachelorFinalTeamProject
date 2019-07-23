using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CEditorManagerBootstrapper : Bootstrapper
{
    public InputMaster Input;

    public float Dimension = 30f;
    [Range(0.1f, 0.5f)]
    public float ButtonDelayAmount;
    public CameraMovement CamMovement;
    public CEditorMenu EditorMenu;
    public TextMeshProUGUI CurrentWorkingPosition, LookDirection;

    private CEditorManager cEditorManager;

    private void OnEnable() => cEditorManager.RegisterBootstrapper(this);
    private void OnDisable() => cEditorManager.UnregisterBootstrapper();

    private void Awake()
    {
        cEditorManager = CEditorManager.Instance;
    }

    private void Update() => cEditorManager.CEditorUpdate();
}
