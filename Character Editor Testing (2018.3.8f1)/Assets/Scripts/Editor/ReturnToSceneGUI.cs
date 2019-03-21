using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public sealed class ReturnToSceneGUI
{
    public SceneSetupWrapper PreviousSceneSetup;
    public GameObject PrefabInstance;
    private GUIStyle style;

    public ReturnToSceneGUI()
    {
        SceneView.onSceneGUIDelegate += RenderSceneGUI;
        style = new GUIStyle();
        style.margin = new RectOffset(10, 10, 10, 10);
    }

    public void RenderSceneGUI(SceneView sceneview)
    {
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(20, 20, 150, 300), style);
        var rect = EditorGUILayout.BeginVertical();
        GUI.Box(rect, GUIContent.none);

        //if (GUILayout.Button("Save Mesh", new GUILayoutOption[0]))
        //{
        //    //PrefabUtility.ReplacePrefab(PrefabInstance, PrefabUtility.GetPrefabParent(PrefabInstance), ReplacePrefabOptions.ConnectToPrefab);

        //    CharacterCreatorEW.CloseWindow();
        //    SceneView.onSceneGUIDelegate -= RenderSceneGUI;
        //    PreviousSceneSetup.OpenSetup();
        //}

        if (GUILayout.Button("Discard and Close", new GUILayoutOption[0]))
        {
            EditorUtility.FocusProjectWindow();
            CharacterCreatorEW.CloseWindow(false);
        }

        EditorGUILayout.EndVertical();
        GUILayout.EndArea();
        Handles.EndGUI();
    }
}
