﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshGenerator : MonoBehaviour
{
    public Vector3Int[] CubePositions;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    private CubeMesh[] cubes;

    private Vector3[] allPossibleVertices;

    [SerializeField] private Vector3Int BuildingRange = new Vector3Int(10,10,10);

    public void OnEditorStart()
    {
        allPossibleVertices = new Vector3[BuildingRange.x * BuildingRange.y * BuildingRange.z];
        for (int i = 0, x = 0; x < BuildingRange.x; x++)
        {
            for (int y = 0; y < BuildingRange.y; y++)
            {
                for (int z = 0; z < BuildingRange.z; z++)
                {
                    allPossibleVertices[i] = new Vector3(x-.5f, y-.5f, z-.5f);
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
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();
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
        cubes = new CubeMesh[CubePositions.Length];
        for (int amount = 0; amount < cubes.Length; amount++)
        {
            cubes[amount] = new CubeMesh(CubePositions[amount]);
        }
        if(cubes != null)
        {
            vertices = ExtractVerticesFromCubes(); // Always extract the vertices first!
            triangles = ExtractTrianglesFromCube();
            OptimizeMesh();
        }
    }

    public void RemoveShape()
    {
        mesh.Clear();
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
                if(!verts.Contains(cubes[amount].Vertices[pos]))
                    verts.Add(cubes[amount].Vertices[pos]);
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
                tris[position] = (amount * 8) + cubes[amount].Triangles[pos];
                position++;
            }
        }
        return tris;
    }

    private void OptimizeMesh()
    {
    }

    private void OnDrawGizmos()
    {
        if (allPossibleVertices == null)
            return;

        for (int i = 0; i < allPossibleVertices.Length; i++)
        {
            Gizmos.DrawSphere(allPossibleVertices[i], .05f);
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
}
