using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;

public class CEditorInput
{
    public Vector2 Dpad, LeftStick, RightStick = Vector2.zero;
    public float LeftTrigger, RightTrigger = 0;
    public bool LeftShoulder, RightShoulder, LeftStickPress, RightStickPress, UpButton, DownButton, LeftButton, RightButton, Select, StartButton;

    private InputMaster input;

    public void Start(InputMaster input)
    {
        this.input = input;
        input.CEditor.LeftStick.performed += LeftStickInput;
        input.CEditor.RightStick.performed += RightStickInput;
        input.CEditor.LeftTrigger.performed += LeftTriggerInput;
        input.CEditor.RightTrigger.performed += RightTriggerInput;
        input.CEditor.LeftStickPress.performed += LeftStickPressInput;
        input.CEditor.RightStickPress.performed += RightStickPressInput;

        input.CEditor.Start.performed += OpenMenu;
    }

    public void End()
    {
        input.CEditor.LeftStick.performed -= LeftStickInput;
        input.CEditor.RightStick.performed -= RightStickInput;
        input.CEditor.LeftTrigger.performed -= LeftTriggerInput;
        input.CEditor.RightTrigger.performed -= RightTriggerInput;
        input.CEditor.LeftStickPress.performed -= LeftStickPressInput;
        input.CEditor.RightStickPress.performed -= RightStickPressInput;

        input.CEditor.Start.performed -= OpenMenu;
    }

    private void DpadInput(InputAction.CallbackContext ctx)
    {
        Dpad = ctx.ReadValue<Vector2>();
    }
    private void LeftStickInput(InputAction.CallbackContext ctx)
    {
        LeftStick = ctx.ReadValue<Vector2>();
    }
    private void RightStickInput(InputAction.CallbackContext ctx)
    {
        RightStick = ctx.ReadValue<Vector2>();
    }
    private void LeftTriggerInput(InputAction.CallbackContext ctx)
    {
        LeftTrigger = ctx.ReadValue<float>() <= .9f ? 0 : ctx.ReadValue<float>();
    }
    private void RightTriggerInput(InputAction.CallbackContext ctx)
    {
        RightTrigger = ctx.ReadValue<float>() <= .9f ? 0 : ctx.ReadValue<float>();
    }

    private void LeftStickPressInput(InputAction.CallbackContext ctx)
    {
        LeftStickPress = !LeftStickPress;
    }

    private void RightStickPressInput(InputAction.CallbackContext ctx)
    {
        RightStickPress = !RightStickPress;
    }

    private void OpenMenu(InputAction.CallbackContext ctx)
    {
        if (!CEditorManager.Instance.LoadingEditor)
        {
            LeftButton = !LeftButton;
            CEditorManager.Instance.ManageMenu();
        }
    }

    public void OpenMenu()
    {
        LeftButton = !LeftButton;
        CEditorManager.Instance.ManageMenu();
    }

    public void BackToMainMenu()
    {
        CEditorManager.Instance.PlayEditorSound(EditorEffectSound.CONFIRM);
        GameManager.Instance.LoadScene(0);
    }
}
