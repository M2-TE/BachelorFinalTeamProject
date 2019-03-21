using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CharacterBuilder
{
    internal GameObject NewCharacter;

    private List<Vector3Int> cubePositions;

    public CharacterBuilder()
    {
        NewCharacter = new GameObject("New Character");
        NewCharacter.hideFlags = HideFlags.NotEditable;
        new PrimitiveCube().transform.parent = NewCharacter.transform;
    }

    public void Update()
    {
        if (CharacterCreatorEW.Open)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;
            /*
            if(Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out hit,1000,9))
            {
                Debug.Log("yay");
            }
            */
        }
    }

    public CharacterMesh GetCurrentMesh
    {
        get
        {
            CharacterMesh cMesh = new CharacterMesh();
            cMesh.Dimesion = new Vector3Int(10, 20, 10);
            cMesh.CubePositions = new Vector3Int[10];
            for (int i = 0; i <cMesh.CubePositions.Length; i++)
            {
                cMesh.CubePositions[i] = new Vector3Int(i, 0, 0);
            }
            return cMesh;
        }
    }
}
