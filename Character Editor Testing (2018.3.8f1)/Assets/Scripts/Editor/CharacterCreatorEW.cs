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

    internal Scene currentWorkingScene;

    private GameObject newCharacter;

    private bool characterTypeChosen = false;
    private Vector2 scrollPosition;
    private Tool currentTool = Tool.Additive;

    private void OnGUI()
    {
        GUILayout.Space(20);
        if (!characterTypeChosen)
        {
            GUILayout.Label("What kind of character do you want to create?");
            if (GUILayout.Button("Humanoid"))
            {
                OpenEditScene();
                characterTypeChosen = true;

                Debug.Log("Not implemented yet");
            }
            if (GUILayout.Button("Alien"))
            {
                OpenEditScene();
                characterTypeChosen = true;
                InitNewChar();
            }
        }
        else
        {
            if (GUILayout.Button("Back"))
            {
                CloseEditScene();
                characterTypeChosen = false;
            }
        }
        
        GUILayout.Space(60);

        if (GUILayout.Button("Close Editor"))
        {
            Close();
        }
    }

    public void OnScene(SceneView sceneview)
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        Handles.BeginGUI();
        GUI.Box(new Rect(10, 10, 120, sceneview.position.size.y-40), "Tools");
        if (GUI.Button(new Rect(20, 40, 100, 20), "Add Cube"))
        {
            currentTool = Tool.Additive;
        }
        if (GUI.Button(new Rect(20, 70, 100, 20), "Remove Cube"))
        {
            currentTool = Tool.Subtractive;
        }
        GUI.Label(new Rect(sceneview.position.size.x/2 - 100, 10, 200, 40), "Edit Mode: "+currentTool.ToString(), new GUIStyle() { alignment = TextAnchor.MiddleCenter });
        Handles.EndGUI();

        if(Event.current != null)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray,out RaycastHit hit,100) && hit.transform.gameObject.GetComponent<CharacterCubeModule>())
            {
                Handles.DrawWireCube(hit.transform.position + hit.normal, Vector3.one);
            }
        }
    }

    private void InitNewChar()
    {
        newCharacter = new GameObject("New Character");
        newCharacter.hideFlags = HideFlags.NotEditable;
        var primCube = new PrimitiveCube();
        primCube.transform.parent = newCharacter.transform;
        primCube.gameObject.GetComponent<CharacterCubeModule>().editable = false;
    }

    private void OnDestroy()
    {
        CloseEditScene();
        Instance = null;
    }

    #region static methods

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
        Instance.titleContent = new GUIContent("Character Creator");
        Instance.Show();
    }

    private static void OpenEditScene()
    {
        if (Instance == null)
            return;
        Instance.currentWorkingScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneView.onSceneGUIDelegate += Instance.OnScene;
    }

    private static void CloseEditScene()
    {
        if (Instance == null || Instance.currentWorkingScene == null)
            return;
        SceneView.onSceneGUIDelegate -= Instance.OnScene;
        if (EditorUtility.DisplayDialog("Save Character Before Closing?", "Do you wish to save the current character in a file before exiting the editor?\nWARNING: Unsaved changes will disappear!", "Save", "Discard Changes"))
        {
            Debug.Log("Character Saved In A File");
        }
        EditorSceneManager.CloseScene(Instance.currentWorkingScene, true);
    }

    #endregion
}