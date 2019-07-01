using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "NewCScriptableMap", order = 0)]
public class CScriptableMap : ScriptableObject
{
    public int MapID;
    public GameObject Map;
    public Vector2[] SpawnPoints;
    public Vector2[] ItemSpawnPoints;
}
