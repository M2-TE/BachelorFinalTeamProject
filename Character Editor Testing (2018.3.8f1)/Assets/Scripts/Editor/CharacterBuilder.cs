using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CharacterBuilder : Editor
{
    internal GameObject NewCharacter;

    public void OnEnable()
    {
        NewCharacter = new GameObject("New Character");
        NewCharacter.hideFlags = HideFlags.NotEditable;
        new PrimitiveCube().transform.parent = NewCharacter.transform;
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
