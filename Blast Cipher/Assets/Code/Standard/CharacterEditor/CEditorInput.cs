using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;

public class CEditorInput : MonoBehaviour
{
    public Vector2 Dpad, LeftStick, RightStick;
    public bool LeftShoulder, RightShoulder, LeftStickPress, RightStickPress, LeftTrigger, RightTrigger, UpButton, DownButton, LeftButton, RightButton, Select, StartButton;

    public void Start()
    {

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

    private void LeftShoulderInput(InputAction.CallbackContext ctx)
    {
        LeftShoulder = ctx.ReadValue<bool>();
    }

    public override string ToString()
    {
        return "Dpad[" + Dpad.ToString() + "] | LeftStick[" + LeftStick + "] | RightStick[" + RightStick + "] | LeftShoulder[" + (LeftShoulder ? "true" : "false") + "] | RightShoulder[" + (RightShoulder ? "true" : "false") + "] | LeftStickPress[" + (RightStickPress ? "true" : "false") + "] | RightStickPress[" + (RightStickPress ? "true" : "false") + "] | LeftTrigger[" + (LeftTrigger ? "true" : "false") + "] | RightTrigger[" + (RightTrigger ? "true" : "false") + "] | UpButton[" + (UpButton ? "true" : "false") + "] | DownButton[" + (DownButton ? "true" : "false") + "] | LeftButton[" + (LeftButton ? "true" : "false") + "] | RightButton[" + (RightButton ? "true" : "false") + "] | Select[" + (Select ? "true" : "false") + "] | Start[" + (StartButton ? "true" : "false") + "]";
    }
}
