using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialsHolder : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] renderers;

    public void SetMaterials(Material mat)
    {
        foreach (var renderer in renderers)
        {
            renderer.material = mat;
        }
    }
}
