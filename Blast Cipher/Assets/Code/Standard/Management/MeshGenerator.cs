using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{

    public static Mesh GenerateMeshFromScriptableObject(CScriptableCharacter character)
    {
        Mesh mesh = new Mesh();
        CCubeMesh[] cubesMesh = new CCubeMesh[character.CubePositions.Length];
        for (int amount = 0; amount < cubesMesh.Length; amount++)
        {
            cubesMesh[amount] = new CCubeMesh(character.CubePositions[amount]);
        }

        List<BasicMesh> cubesMeshData = new List<BasicMesh>();
        foreach (var cubeMesh in cubesMesh)
        {
            bool[] neighbours = HasNeighbours(cubeMesh.Position,character);
            cubesMeshData.Add(cubeMesh.GetMeshData(!neighbours[0], !neighbours[1], !neighbours[2], !neighbours[3], !neighbours[4], !neighbours[5]));
        }
        BasicMesh complete = new BasicMesh();
        foreach(var basicMesh in cubesMeshData)
        {
            CombineBasicMeshes(ref complete, basicMesh,character);
        }

        if (cubesMesh != null)
        {
            mesh.Clear();
            mesh.vertices = complete.Vertices.ToArray();
            mesh.triangles = complete.Triangles.ToArray();
            mesh.RecalculateNormals();
        }
        return mesh;
    }

    private static void CombineBasicMeshes(ref BasicMesh first, BasicMesh second, CScriptableCharacter character)
    {
        int triOffset = first.Vertices.Count;
        foreach (var vert in second.Vertices)
        {
            first.Vertices.Add((vert - character.Offset) / character.CharacterScaling);
        }
        foreach (var tri in second.Triangles)
        {
            first.Triangles.Add(tri + triOffset);
        }
    }

    private static bool[] HasNeighbours(Vector3Int position, CScriptableCharacter character)
    {
        return new bool[] { HasCubeAt(position + new Vector3Int(0, 0, 1), character), HasCubeAt(position + new Vector3Int(0, 0, -1), character), HasCubeAt(position + new Vector3Int(1, 0, 0), character), HasCubeAt(position + new Vector3Int(-1, 0, 0), character), HasCubeAt(position + new Vector3Int(0, 1, 0), character), HasCubeAt(position + new Vector3Int(0, -1, 0), character) };
    }

    private static bool HasCubeAt(Vector3Int pos, CScriptableCharacter character)
    {
        for (int i = 0; i < character.CubePositions.Length; i++)
        {
            if (character.CubePositions[i].Equals(pos))
                return true;
        }
        return false;
    }
}
