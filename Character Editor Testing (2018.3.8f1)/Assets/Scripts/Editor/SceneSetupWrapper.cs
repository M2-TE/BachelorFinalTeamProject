using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

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
