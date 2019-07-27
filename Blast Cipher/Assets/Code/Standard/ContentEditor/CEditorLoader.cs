using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEditorLoader : MonoBehaviour
{
    [SerializeField] private Transform characterSpawn;
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private float camspeed = 1f;
    [SerializeField] private float characterDistance = 5f;

    private GameObject[] meshes = new GameObject[0];

    private int pos = 0;

    private Vector3 startingPos = new Vector3(0,0,4f);

    private bool currentlyMoving = false;

    public int Pos => Mathf.Abs(pos);

    public void Activate()
    {
        meshes = new GameObject[GameManager.Instance.ContentHolder.Characters.Count];
        for (int i = 0; i < meshes.Length; i++)
        {
            meshes[i] = GameObject.Instantiate(objectPrefab,characterSpawn);
            meshes[i].transform.position = meshes[i].transform.position + (i * characterDistance * Vector3.right);
            meshes[i].GetComponent<MeshFilter>().mesh = MeshGenerator.GenerateMeshFromScriptableObject(GameManager.Instance.ContentHolder.Characters[i]);
            meshes[i].GetComponent<MeshRenderer>().material = GameManager.Instance.CharacterMaterials[GameManager.Instance.ContentHolder.Characters[i].CharacterColor];
            meshes[i].gameObject.layer = 17;
        }
    }

    public IEnumerator MoveCamera(bool increment)
    {
        if (!currentlyMoving)
        {
            currentlyMoving = true;
            pos += (increment ? 1 : -1);
            Vector3 start = characterSpawn.position;
            Vector3 finish = start - (Vector3.right * (increment ? characterDistance : -characterDistance));
            float time = Time.time;
        
            while (pos >= 0 && pos < meshes.Length && characterSpawn.position != finish)
            {
                float distance = (Time.time - time) * camspeed * 10;
                characterSpawn.position = Vector3.Lerp(start, finish, distance / characterDistance);
                yield return new WaitForEndOfFrame();
            }
            if (pos >= 0 && pos < meshes.Length)
                CEditorManager.Instance.PlayEditorSound(EditorEffectSound.SWIPE);
            else
                CEditorManager.Instance.PlayEditorSound(EditorEffectSound.SWIPEERROR);
            pos = pos < 0 ? 0 : pos >= meshes.Length ? meshes.Length - 1 : pos;
            currentlyMoving = false;
        }
    }

    public void Deactivate()
    {
        foreach (var mesh in meshes)
        {
            Destroy(mesh);
        }
        characterSpawn.position = startingPos;
        Debug.Log(characterSpawn.position);
        meshes = new GameObject[0];
        pos = 0;
    }
}
