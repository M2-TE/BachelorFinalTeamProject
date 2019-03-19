using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterCreatorEW : EditorWindow
{
    public static CharacterCreatorEW Instance { get; private set; }
    public static bool Open => Instance != null;

    [MenuItem("Window/Character Creator")]
    private static void Init()
    {
        if (Open)
            Instance.Focus();
        else
            OpenWindow();
    }

    public static void OpenWindow()
    {
        EditorSceneManager.SaveOpenScenes();
        var currentScenes = new SceneSetupWrapper();
        currentScenes.TakeSnapshot();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneView.lastActiveSceneView.FrameSelected();

        var returnHandler = new ReturnToSceneGUI();
        returnHandler.PreviousSceneSetup = currentScenes;

        Instance = (CharacterCreatorEW)EditorWindow.GetWindow(typeof(CharacterCreatorEW));
        Instance.Show();
    }

    public static void CloseWindow()
    {
        Instance.Close();
    }
}