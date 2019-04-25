using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;


[InitializeOnLoad]
public class CharacterCreatorEW : EditorWindow
{
    public static CharacterCreatorEW Instance { get; private set; }
    public static bool Open => Instance != null;

    internal Scene currentWorkingScene;

    private GameObject newCharacter;

    private bool characterTypeChosen = false;
    private Tool currentTool = Tool.Additive;

    private string characterName = "New Character";
    private CharacterMesh character;
    private List<Vector3Int> currentCubes = new List<Vector3Int>();

    private void OnGUI()
    {
        GUILayout.Space(20);
        if (!characterTypeChosen)
        {
            GUILayout.Label("Do you wish to create a new character\nor work on an already exsisting one?");
            if (GUILayout.Button("Already exsisting character"))
            {
                OpenEditScene();
                characterTypeChosen = true;

                Debug.Log("Not implemented yet");
            }
            if (GUILayout.Button("New character"))
            {
                OpenEditScene();
                characterTypeChosen = true;
                InitNewChar();
            }
        }
        else
        {
            GUILayout.Label("Give your character a unique name:");
            characterName = GUILayout.TextField(characterName, 20);

            GUILayout.Space(10);

            if (GUILayout.Button("Save"))
            {
                SaveCharacter();
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Back"))
            {
                CloseEditScene();
                characterTypeChosen = false;
            }
        }
        
        GUILayout.Space(60);
        if (GUILayout.Button("RemoveMistakes"))
        {
            DestroyImmediate(GameObject.Find("New Character"));
        }
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
        if (Event.current != null)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100) && hit.transform.gameObject.GetComponent<CharacterCubeModule>())
            {
                if (currentTool.Equals(Tool.Additive))
                {
                    Handles.DrawWireCube(hit.transform.position + hit.normal, Vector3.one);
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        AddCube((hit.transform.position + hit.normal));
                }
                else if (currentTool.Equals(Tool.Subtractive))
                {
                    Handles.DrawWireCube(hit.transform.position, new Vector3(1, 1, 1));
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        RemoveCube(hit.collider.gameObject);
                }
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

        character = new CharacterMesh();
        currentCubes.Add(Vector3Int.zero);
    }

    private void SaveCharacter()
    {
        if (character == null)
            return;
        character.CubePositions = currentCubes.ToArray();
        character.Dimesion = CalcDimensions(currentCubes.ToArray());

        if (!AssetDatabase.Contains(character))
            AssetDatabase.CreateAsset(character, "Assets/" +characterName+ ".asset");
        else
            AssetDatabase.SaveAssets();
    }

    private void AddCube(Vector3 position)
    {
        var primCube = new PrimitiveCube();
        primCube.transform.parent = newCharacter.transform;
        primCube.transform.position = position;

        currentCubes.Add(ConvertVec(position));
    }

    private void RemoveCube(GameObject Cube)
    {
        if(Cube.GetComponent<CharacterCubeModule>() && Cube.GetComponent<CharacterCubeModule>().editable)
        {
            currentCubes.Remove(ConvertVec(Cube.transform.position));
            DestroyImmediate(Cube);
        }
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
            Instance.SaveCharacter();
        }
        EditorSceneManager.CloseScene(Instance.currentWorkingScene, true);
    }

    private static Vector3Int CalcDimensions(Vector3Int[] cubes)
    {
        int highestDimension = 1;

        for (int x = 0; x < cubes.Length; x++)
        {
            highestDimension = cubes[x].x > highestDimension+1 ? cubes[x].x : highestDimension;
        }
        for (int z = 0; z < cubes.Length; z++)
        {
            highestDimension = cubes[z].z > highestDimension+1 ? cubes[z].z : highestDimension;
        }
        for (int y = 0; y < cubes.Length; y++)
        {
            highestDimension = (int)cubes[y].y/2 > highestDimension+1 ? (int)(cubes[y].y / 2) : highestDimension;
        }

        return new Vector3Int(highestDimension, highestDimension * 2, highestDimension);
    }

    private static Vector3Int ConvertVec(Vector3 vector)
    {
        return new Vector3Int((int)vector.x, (int)vector.y, (int)vector.z);
    }
    #endregion
}