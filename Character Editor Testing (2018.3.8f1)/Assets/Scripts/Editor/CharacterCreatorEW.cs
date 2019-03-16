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

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneView.lastActiveSceneView.FrameSelected();

        var returnHandler = new ReturnToSceneGUI();
        returnHandler.PreviousSceneSetup = currentScenes;
    }
}