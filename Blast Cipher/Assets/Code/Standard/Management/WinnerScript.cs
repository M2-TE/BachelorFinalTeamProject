using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Experimental.Input.InputAction;

public class WinnerScript : MonoBehaviour
{
    [SerializeField] MeshFilter[] filter;
    [SerializeField] GameObject[] parents;
    [SerializeField] InputMaster input;

    private void Start()
    {
        List<Mesh> meshes = GameManager.Instance.WinnerMeshes;
        GameManager.Instance.WinnerMeshes.Clear();
        int i = 0;
        foreach (var item in filter)
        {
            if (i < meshes.Count)
                item.mesh = meshes[i];
            else
                parents[i].SetActive(false);
            i++;
            
        }
    }

    private void OnEnable()
    {
        input.CEditor.SouthButton.performed += BackToMainMenu;
    }
    private void OnDisable()
    {
        input.CEditor.SouthButton.performed -= BackToMainMenu;
    }

    public void BackToMainMenu(CallbackContext ctx)
    {
        GameManager.Instance.LoadScene(0);
    }
}
