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

    internal CharacterBuilder cBuilder;
    internal Scene currentWorkingScene;

    private void OnDestroy()
    {
        if(EditorUtility.DisplayDialog("Save Character Before Closing?","Do you wish to save the current character in a file before exiting the editor?\nWARNING: Unsaved changes will disappear!","Save","Discard Changes"))
        {
            Debug.Log("Character Saved In A File");
        }

        EditorSceneManager.CloseScene(currentWorkingScene,true);

        Instance = null;
    }

    private void OnGUI()
    {
        if(GUILayout.Button("Close Editor"))
        {
            Close();
        }
        GUI.Box(new Rect(10, 10, 100, 90), "Loader Menu");

        // Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
        if (GUI.Button(new Rect(20, 40, 80, 20), "Level 1"))
        {
            Application.LoadLevel(1);
        }

        // Make the second button.
        if (GUI.Button(new Rect(20, 70, 80, 20), "Level 2"))
        {
            Application.LoadLevel(2);
        }
    }

    [MenuItem("Window/Character Creator")]
    private static void Init()
    {
        if (Open)
            Instance.Focus();
        else
            NewWindow();
    }

    public static void NewWindow()
    {
        EditorSceneManager.SaveOpenScenes();

        Instance = (CharacterCreatorEW)EditorWindow.GetWindow(typeof(CharacterCreatorEW));
        Instance.currentWorkingScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        Instance.Show();

        
    }
}