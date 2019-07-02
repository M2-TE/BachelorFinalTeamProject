using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CScriptableHolder : ScriptableObject
{
    private static CScriptableHolder instance;
    public static CScriptableHolder Instance { get => instance ?? (instance = ScriptableObject.CreateInstance<CScriptableHolder>()); }

    [System.NonSerialized] public List<CScriptableCharacter> Characters = new List<CScriptableCharacter>();
    [System.NonSerialized] public List<CScriptableMap> Maps = new List<CScriptableMap>();

    public List<string> CharacterPaths = new List<string>();
    public List<string> MapPaths = new List<string>();

    public void AddCharacter(CScriptableCharacter character)
    {
        Characters.Add(character);
        CharacterPaths.Add("/Characters/" + character.CharacterID + ".json");
    }

    public void AddMap(CScriptableMap map)
    {
        Maps.Add(map);
        MapPaths.Add("/Maps/" + map.MapID + ".json");
    }

}
