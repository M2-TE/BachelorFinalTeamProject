using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimitiveCube
{
    public GameObject Instance { get; private set; }
    public GameObject gameObject => Instance;
    public Transform transform => Instance.transform;

    public PrimitiveCube()
    {
        Instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Instance.hideFlags = HideFlags.NotEditable;
        Instance.AddComponent<CharacterCubeModule>().Init();
        Instance.tag = "BodyPart";
    }
}
