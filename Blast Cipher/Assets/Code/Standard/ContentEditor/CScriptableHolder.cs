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
        bool existingChar = false;
        foreach (var c in Characters)
        {
            if (c.CharacterID.Equals(character.CharacterID))
            {
                existingChar = true;
                Characters.Remove(c);
            }
        }
        Characters.Add(character);
        if (!existingChar)
            CharacterPaths.Add("/Characters/" + character.CharacterID + ".json");

    }

    public void RemoveCharacter(CScriptableCharacter character)
    {
        int pos = 0;
        foreach (var c in Characters)
        {
            if (c.CharacterID.Equals(character.CharacterID))
            {
                Characters.RemoveAt(pos);
                CharacterPaths.RemoveAt(pos);
                //CharacterPaths.Remove("/Characters/" + c.CharacterID + ".json");
            }
            pos++;
        }
    }

    public void AddMap(CScriptableMap map)
    {
        Maps.Add(map);
        MapPaths.Add("/Maps/" + map.MapID + ".json");
    }

}
