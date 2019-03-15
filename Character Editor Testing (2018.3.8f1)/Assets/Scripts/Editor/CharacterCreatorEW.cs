using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterCreatorEW : EditorWindow
{

    [MenuItem("Window/Character Creation")]
    private static void Init()
    {
        EditorSceneManager.SaveOpenScenes();
        var currentScenes = new SceneSetupWrapper();
        currentScenes.TakeSnapshot();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        SceneView.lastActiveSceneView.FrameSelected();

        var returnHandler = new ReturnToSceneGUI();
        returnHandler.PreviousSceneSetup = currentScenes;
    }

    private void OnGUI()
    {
        GUILayout.Label("Create Your Character");
    }
}

public sealed class SceneSetupWrapper
{
    private string[] scenePaths;
    private bool[] scenesLoaded;
    private int activeSceneIndex;

    public void TakeSnapshot()
    {
        int sceneCount = SceneManager.sceneCount;
        scenePaths = new string[sceneCount];
        scenesLoaded = new bool[sceneCount];
        var activeScene = SceneManager.GetActiveScene();
        for (int i = 0; i < sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            scenePaths[i] = scene.path;
            scenesLoaded[i] = scene.isLoaded;
            if (scene == activeScene)
            {
                activeSceneIndex = i;
            }
        }
    }

    public void OpenSetup()
    {
        if (scenePaths.Length == 0)
        {
            Debug.LogError("Can't open scene setup, no data stored.");
            return;
        }
        try
        {
            bool canceled = false;
            int sceneCount = scenePaths.Length;
            canceled = EditorUtility.DisplayCancelableProgressBar("Loading scenes",
                    "Loading scenes from Setup", (float)0 / sceneCount);

            Scene activeScene = EditorSceneManager.OpenScene(scenePaths[0]);

            if (canceled)
            {
                EditorUtility.ClearProgressBar();
                return;
            }
            for (int i = 1; i < sceneCount; i++)
            {
                canceled = EditorUtility.DisplayCancelableProgressBar("Loading scenes",
                    "Loading scenes from Setup", (float)i / sceneCount);
                if (canceled)
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                var openSceneMode = scenesLoaded[i] ? OpenSceneMode.Additive : OpenSceneMode.AdditiveWithoutLoading;
                var scene = EditorSceneManager.OpenScene(scenePaths[i], openSceneMode);
                if (activeSceneIndex == i)
                {
                    activeScene = scene;
                }
            }
            SceneManager.SetActiveScene(activeScene);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
}

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
        GUILayout.BeginArea(new Rect(20, 20, 180, 300), style);
        var rect = EditorGUILayout.BeginVertical();
        GUI.Box(rect, GUIContent.none);

        if (GUILayout.Button("Create Character Mesh", new GUILayoutOption[0]))
        {
            //PrefabUtility.ReplacePrefab(PrefabInstance, PrefabUtility.GetPrefabParent(PrefabInstance), ReplacePrefabOptions.ConnectToPrefab);
            SceneView.onSceneGUIDelegate -= RenderSceneGUI;
            PreviousSceneSetup.OpenSetup();
        }

        if (GUILayout.Button("Discard changes", new GUILayoutOption[0]))
        {
            SceneView.onSceneGUIDelegate -= RenderSceneGUI;
            PreviousSceneSetup.OpenSetup();
        }

        EditorGUILayout.EndVertical();
        GUILayout.EndArea();
        Handles.EndGUI();
    }
}