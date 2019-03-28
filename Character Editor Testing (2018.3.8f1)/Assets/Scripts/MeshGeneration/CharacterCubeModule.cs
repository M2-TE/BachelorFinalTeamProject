using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCubeModule : MonoBehaviour
{
    public bool editable = true;

    public void Init()
    {
        gameObject.AddComponent<Rigidbody>().isKinematic = true;
    }
}
