using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CScriptableMap))]
public class CInspectorScriptableMap : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (((CScriptableMap)target).MapID.Length <= 3)
        { 
            EditorGUILayout.Space();
            if (GUILayout.Button("Create new Guid"))
            {
                ((CScriptableMap)target).GenerateNewGuid();
            }
        }
    }
}
