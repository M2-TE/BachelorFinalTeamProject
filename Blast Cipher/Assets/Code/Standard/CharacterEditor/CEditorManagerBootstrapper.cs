using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEditorManagerBootstrapper : Bootstrapper
{
    public InputMaster input;

    [SerializeField] private CameraMovement cameraMovement;


    private CEditorManager cEditorManager;

    private void OnEnable() => cEditorManager.RegisterBootstrapper(this);
    private void OnDisable() => cEditorManager.UnregisterBootstrapper();

    private void Awake()
    {
        cEditorManager = CEditorManager.Instance;
    }

    private void Update() => cEditorManager.CEditorUpdate();
}
