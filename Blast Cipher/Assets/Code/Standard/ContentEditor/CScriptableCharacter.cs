using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "NewCScriptableCharacter",order = 0)]
public class CScriptableCharacter : ScriptableObject
{
    public string CharacterID;
    public int CharacterScaling, CharacterColor;
    public Vector3 Offset;
    public Vector3Int[] CubePositions;

    public void GenerateNewGuid()
    {
        CharacterID = Guid.NewGuid().ToString();
    }
}
