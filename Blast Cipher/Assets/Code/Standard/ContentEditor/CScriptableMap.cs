using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "NewCScriptableMap", order = 0)]
public class CScriptableMap : ScriptableObject
{
    public string MapID;
    public GameObject Map;
    public Vector2[] SpawnPoints;
    public Vector2[] ItemSpawnPoints;

    public void GenerateNewGuid()
    {
        MapID = Guid.NewGuid().ToString();
    }
}
