using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;

public class CreditsManager : MenuManager
{
    private void Start()
    {
        ToggleActivation(false);
    }

    public override void OnConfirmation(InputAction.CallbackContext ctx)
    {
        mainManager.ManageSubmenu(false);
    }

    public override void OnDecline(InputAction.CallbackContext ctx)
    {
        mainManager.ManageSubmenu(false);
    }

    public override void OnDPadInput(InputAction.CallbackContext ctx)
    {
        mainManager.ManageSubmenu(false);
    }

    public override void OnStartPressed(InputAction.CallbackContext ctx)
    {
        mainManager.ManageSubmenu(false);
    }
}
