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

    public static ReturnToSceneGUI returnHandler;

    internal CharacterBuilder cBuilder;

    public void OnEnable()
    {
        cBuilder = new CharacterBuilder();
    }

    public void Update()
    {
        cBuilder.Update();
    }

    private void OnDestroy()
    {
        CloseWindow(true);
    }

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

        returnHandler = new ReturnToSceneGUI
        {
            PreviousSceneSetup = currentScenes
        };

        Instance = (CharacterCreatorEW)EditorWindow.GetWindow(typeof(CharacterCreatorEW));
        Instance.Show();
    }

    public static void CloseWindow(bool onDestroy)
    {
        SceneView.onSceneGUIDelegate -= returnHandler.RenderSceneGUI;
        returnHandler.PreviousSceneSetup.OpenSetup();
        if (!onDestroy) Instance.Close();
        Instance = null;
        returnHandler = null;
    }
}