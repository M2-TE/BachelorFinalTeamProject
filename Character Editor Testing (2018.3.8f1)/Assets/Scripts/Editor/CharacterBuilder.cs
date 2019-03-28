using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class CharacterBuilder : Editor
{
    private CharacterMesh character;

    public void OnSceneGUI()
    {
        GUI.Box(new Rect(10, 10, 100, 90), "Loader Menu");

        if (GUI.Button(new Rect(20, 40, 80, 20), "Level 1"))
        {
            SceneManager.LoadScene(1);
        }
        if (GUI.Button(new Rect(20, 70, 80, 20), "Level 2"))
        {
            SceneManager.LoadScene(2);
        }
    }
}
