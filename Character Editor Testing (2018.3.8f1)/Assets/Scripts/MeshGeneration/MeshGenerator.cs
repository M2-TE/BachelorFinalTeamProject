using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshGenerator : MonoBehaviour
{
    public CharacterMesh Character;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    private CubeMesh[] cubes;

    private Vector3[] allPossibleVertices;

    public bool Generated
    {
        get;
        set;
    }

    public void OnEditorStart()
    {
        if (Character == null)
            return;
        allPossibleVertices = new Vector3[(Character.Dimesion.x+1) * (Character.Dimesion.y+1) * (Character.Dimesion.z+1)];
        for (int i = 0, x = 0; x < Character.Dimesion.x +1; x++)
        {
            for (int y = 0; y < Character.Dimesion.y + 1; y++)
            {
                for (int z = 0; z < Character.Dimesion.z +1; z++)
                {
                    allPossibleVertices[i] = ScaleVector3( new Vector3(x-.5f, y-.5f, z-.5f));
                    i++;
                }
            }
        }
    }

    public void OnEditorDisable()
    {
        allPossibleVertices = null;
    }

    public void CreateMeshInEditor()
    {
        if (Character == null)
            return;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();
        Generated = true;
    }

    //private void CreateShape()
    //{
    //    vertices = new Vector3[]
    //    {
    //        new Vector3(-0.5f,-0.5f,-0.5f),
    //        new Vector3(-0.5f,-0.5f,0.5f),
    //        new Vector3(0.5f,-0.5f,-0.5f),
    //        new Vector3(0.5f,-0.5f,0.5f),
    //        new Vector3(-0.5f,0.5f,-0.5f),
    //        new Vector3(-0.5f,0.5f,0.5f),
    //        new Vector3(0.5f,0.5f,-0.5f),
    //        new Vector3(0.5f,0.5f,0.5f),
    //    };
    //    triangles = new int[]
    //    {
    //        2,1,0,
    //        1,2,3,
    //        4,5,6,
    //        7,6,5,
    //        5,4,0,
    //        1,5,0,
    //        7,5,1,
    //        1,3,7,
    //        6,7,3,
    //        2,6,3,
    //        4,6,2,
    //        0,4,2
    //    };
    //}
   
    private void CreateShape()
    {
        cubes = new CubeMesh[Character.CubePositions.Length];
        for (int amount = 0; amount < cubes.Length; amount++)
        {
            cubes[amount] = new CubeMesh(Character.CubePositions[amount]);
        }
        if(cubes != null)
        {
            vertices = ExtractVerticesFromCubes(); // Always extract the vertices first!
            triangles = ExtractTrianglesFromCube();
        }
    }

    public void RemoveShape()
    {
        if(mesh != null)
        {
            mesh.Clear();
            Generated = false;
        }
    }

    private void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        //MeshUtility.Optimize(mesh);
    }

    private Vector3[] ExtractVerticesFromCubes()
    {
        List<Vector3> verts = new List<Vector3>();
        for (int amount = 0; amount < cubes.Length; amount++)
        {
            for (int pos = 0; pos < 8; pos++)
            {
                Vector3 vec = ScaleVector3(cubes[amount].Vertices[pos]);
                if(!verts.Contains(vec))
                    verts.Add(vec);
            }
        }
        return verts.ToArray();
    }

    private int[] ExtractTrianglesFromCube()
    {
        int[] tris = new int[cubes.Length * 36];
        for (int position = 0, amount = 0; amount < cubes.Length; amount++)
        {
            for (int pos = 0; pos < 36; pos++)
            {
                tris[position] = PositionOfItemInArray(vertices,ScaleVector3(cubes[amount].Vertices[cubes[amount].Triangles[pos]]));
                position++;
            }
        }
        return tris;
    }

    public void OptimizeMesh()
    {

    }

    public void SaveMesh(string name)
    {
        AssetDatabase.SaveAssets();
        if (mesh == null)
        {
            EditorUtility.DisplayDialog("No Mesh", "You must generate a Mesh to save it!", "Ok");
            return;
        }

        var path = EditorUtility.SaveFilePanel("Save Mesh as Asset", Application.dataPath, name, "asset");
        if (path.Length != 0)
        {
            try
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
                if (AssetDatabase.Contains(mesh))
                {
                    CreateMeshInEditor();
                }
                AssetDatabase.CreateAsset(mesh, path);
                return;
            }
            catch
            {
                Debug.Log("Failed to create Asset");
            }
        }
        EditorUtility.DisplayDialog("No Path found", "The selected Path is invalid or could not be found!\nTipp: The Mesh has to be saved in the Asset folder of the project!", "Ok");
    }

    private void OnDrawGizmos()
    {
        if (allPossibleVertices == null)
            return;

        for (int i = 0; i < allPossibleVertices.Length; i++)
        {
            Gizmos.DrawCube(allPossibleVertices[i], new Vector3(.1f / Character.Dimesion.x, .1f / Character.Dimesion.y, .1f / Character.Dimesion.z));
        }
    }

    private List<T> ArrayToList<T>(T[] array)
    {
        List<T> list = new List<T>();
        foreach (var item in array)
        {
            list.Add(item);
        }
        return list;
    }

    private int PositionOfItemInArray<T>(T[] array, T item)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (item.Equals(array[i]))
                return i;
        }
        return -1;
    }

    private Vector3 ScaleVector3(Vector3 vector)
    {
        Vector3 outVector = new Vector3(vector.x * (1f / Character.Dimesion.x), vector.y * (2f / Character.Dimesion.y), vector.z *(1f / Character.Dimesion.z));
        return outVector;
    }
}
