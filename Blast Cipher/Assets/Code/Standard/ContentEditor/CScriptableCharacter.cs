using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "NewCScriptableCharacter",order = 0)]
public class CScriptableCharacter : ScriptableObject
{
    public int CharacterID;
    public int CharacterScaling;
    public Vector3Int[] CubePositions;
}
