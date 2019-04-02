using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu] // only for debug reasons
public class CharacterMesh : ScriptableObject
{
    [Tooltip("Cubes per axis: x cubes per 1 (2 for height) unity unit(s)")]
    public Vector3Int Dimesion;
    public Vector3Int[] CubePositions; // should be changed to internal when done debugging

}
