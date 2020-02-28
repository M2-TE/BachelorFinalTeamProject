using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;

public abstract class MenuManager : MonoBehaviour
{
    [SerializeField] protected MenuSelectionManager mainManager;
    [SerializeField] protected InputMaster input;
    [SerializeField] protected Transform toggleNode;
    [SerializeField] protected Material defaultMat, highlightedMat;

    public virtual void ToggleActivation(bool setActive)
    {
        toggleNode.gameObject.SetActive(setActive);
    }

    public abstract void OnDPadInput(InputAction.CallbackContext ctx);
    public abstract void OnConfirmation(InputAction.CallbackContext ctx);
    public abstract void OnDecline(InputAction.CallbackContext ctx);
    public abstract void OnStartPressed(InputAction.CallbackContext ctx);
}
