using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Experimental.Input.InputAction;

public class WinnerScript : MonoBehaviour
{
    [SerializeField] private MeshFilter[] filter;
    [SerializeField] private GameObject[] parents;
    [SerializeField] private InputMaster input;
    [SerializeField] private Material[] TeamColors;

    private void Start()
    {
        List<Mesh> meshes = GameManager.Instance.WinnerMeshes;
        int winnerTeamColor = GameManager.Instance.WinnerColor;
        int i = 0;
        foreach (var item in filter)
        {
            if (i < meshes.Count)
            {
                item.mesh = meshes[i];
                item.gameObject.GetComponent<MeshRenderer>().material = GameManager.Instance.CharacterMaterials[winnerTeamColor];
            }
            else
                parents[i].SetActive(false);
            i++;
        }
        GameManager.Instance.WinnerMeshes.Clear();
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
