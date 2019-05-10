using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MenuManager : MonoBehaviour
{
    [SerializeField] protected Transform ToggleNode;
    [SerializeField] protected Material defaultMat, highlightedMat;

    protected bool activated = false;

    public abstract bool Activated { get; set; }

    public abstract void ChangeState(bool increment);

    protected abstract void SetMaterials<T>(T defaultMatState, T highlightedMatState);
    protected abstract void SetMaterial<T>(T state, Material mat);

    protected virtual void ToggleActivation(bool setActive)
    {
        ToggleNode.gameObject.SetActive(setActive);
        activated = setActive;
    }
}
