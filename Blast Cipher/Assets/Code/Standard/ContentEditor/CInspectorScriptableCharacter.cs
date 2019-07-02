using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CScriptableCharacter))]
public class CInspectorScriptableCharacter : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(((CScriptableCharacter)target).CharacterID.Length <= 3)
        {
            EditorGUILayout.Space();
            if(GUILayout.Button("Create new Guid"))
            {
                ((CScriptableCharacter)target).GenerateNewGuid();
            }
        }
    }
}
