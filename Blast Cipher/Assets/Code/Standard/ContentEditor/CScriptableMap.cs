using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CScriptableMap : ScriptableObject
{
    public int MapID;
    public GameObject Map;
    public Vector2[] SpawnPoints;
    public Vector2[] ItemSpawnPoints;
}
