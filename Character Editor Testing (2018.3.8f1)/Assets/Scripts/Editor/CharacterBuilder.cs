using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CharacterBuilder : MonoBehaviour
{
    internal GameObject NewCharacter;

    private CharacterMesh character;

    public void Init()
    {
        NewCharacter = new GameObject("New Character");
        NewCharacter.hideFlags = HideFlags.NotEditable;
        new PrimitiveCube().transform.parent = NewCharacter.transform;
    }

    public void Update(Event current)
    {
        if (CharacterCreatorEW.Open)
        {

            Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);
            RaycastHit hit;

            if(Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out hit,1000,9))
            {
                Debug.Log("yay");
            }
        }
    }
}
