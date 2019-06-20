using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEditorManager
{
    private CEditorManager() { }
    private static CEditorManager instance;
    public static CEditorManager Instance { get => instance ?? (instance = new CEditorManager()); }

    private CEditorManagerBootstrapper bootstrapper;
    public CEditorInput cEditorInput { get; private set; }

    internal void RegisterBootstrapper(CEditorManagerBootstrapper bootstrapper)
    {
        this.bootstrapper = bootstrapper;
        cEditorInput = new CEditorInput();
        cEditorInput.Start(this.bootstrapper.input);
    }
    internal void UnregisterBootstrapper()
    {
        cEditorInput = null;
        bootstrapper = null;
    }

    internal void CEditorUpdate()
    {
        if(bootstrapper != null)
        {
            cEditorInput.Update();
        }
    }
}
