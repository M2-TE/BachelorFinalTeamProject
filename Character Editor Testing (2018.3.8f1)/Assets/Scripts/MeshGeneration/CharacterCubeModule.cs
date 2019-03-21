using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCubeModule : MonoBehaviour
{
    public void Init()
    {
        gameObject.AddComponent<Rigidbody>().isKinematic = true;
    }
}
