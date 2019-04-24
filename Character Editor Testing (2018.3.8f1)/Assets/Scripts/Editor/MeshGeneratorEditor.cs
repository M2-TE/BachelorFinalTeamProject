using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshGenerator))]
public class MeshGeneratorEditor : Editor
{

    private void OnEnable()
    {
        ((MeshGenerator)target).OnEditorStart();
    }

    private void OnDisable()
    {
        ((MeshGenerator)target).OnEditorDisable();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        BaseActions();
    }

    private void BaseActions()
    {
        MeshGenerator myScript = (MeshGenerator)target;
        if(GUILayout.Button("Generate Mesh"))
        {
            myScript.CreateMeshInEditor();
        }
        else if (GUILayout.Button("Remove Mesh"))
        {
            myScript.RemoveShape();
        }

        if (myScript.Generated)
        {
            if(GUILayout.Button("Optimize Mesh"))
                myScript.OptimizeMesh();
            GUILayout.Space(10);
            if (GUILayout.Button("Save Mesh"))
                myScript.SaveMesh(myScript.Character.name);
        }
    }
}
