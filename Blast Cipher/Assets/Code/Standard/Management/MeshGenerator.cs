using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{

    public static Mesh GenerateMeshFromScriptableObject(CScriptableCharacter character)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices;
        int[] triangles;
        CCubeMesh[] cubesMesh = new CCubeMesh[character.CubePositions.Length];
        for (int amount = 0; amount < cubesMesh.Length; amount++)
        {
            cubesMesh[amount] = new CCubeMesh(character.CubePositions[amount]);
        }
        if (cubesMesh != null)
        {
            vertices = ExtractVerticesFromCubicMesh(cubesMesh, character.CharacterScaling);
            triangles = ExtractTrianglesFromCubicMesh(cubesMesh, character, vertices);
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
        }
        return mesh;
    }

    private static Vector3[] ExtractVerticesFromCubicMesh(CCubeMesh[] cubesMesh, int scaling)
    {
        List<Vector3> verts = new List<Vector3>();
        for (int amount = 0; amount < cubesMesh.Length; amount++)
        {
            for (int pos = 0; pos < 8; pos++)
            {
                Vector3 vec = ScaleVector3((cubesMesh[amount].Vertices[pos]),scaling);
                if (!verts.Contains(vec))
                    verts.Add(vec);
            }
        }
        return verts.ToArray();
    }

    private static int[] ExtractTrianglesFromCubicMesh(CCubeMesh[] cubesMesh, CScriptableCharacter character, Vector3[] vertices)
    {
        List<int> tris = new List<int>();
        for (int amount = 0; amount < cubesMesh.Length; amount++)
        {
            for (int direction = 0, position = 0; direction < 6; direction++)
            {
                for (int pos = 0; pos < 6; pos++, position++)
                {
                    if (!HasNeighbor(direction, character.CubePositions[amount],character))
                        tris.Add(PositionOfItemInArray(vertices, ScaleVector3(cubesMesh[amount].Vertices[cubesMesh[amount].Triangles[position]], character.CharacterScaling)));
                }
            }
        }
        return tris.ToArray();
    }

    private static bool HasNeighbor(int side, Vector3Int position, CScriptableCharacter character) // 0Front, 1Back, 2Left, 3Right, 4Up, 5Down
    {
        switch (side)
        {
            case 0:
                return HasCubeAt(position + new Vector3Int(0, 0, 1),character);
            case 1:
                return HasCubeAt(position + new Vector3Int(0, 0, -1), character);
            case 2:
                return HasCubeAt(position + new Vector3Int(1, 0, 0), character);
            case 3:
                return HasCubeAt(position + new Vector3Int(-1, 0, 0), character);
            case 4:
                return HasCubeAt(position + new Vector3Int(0, 1, 0), character);
            case 5:
                return HasCubeAt(position + new Vector3Int(0, -1, 0), character);
            default:
                Debug.Log("Error");
                break;
        }
        return false;
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

    private static int PositionOfItemInArray<T>(T[] array, T item)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (item.Equals(array[i]))
                return i;
        }
        return -1;
    }

    private static Vector3 ScaleVector3(Vector3 vector, int scaling)
    {
        Vector3 outVector = new Vector3(vector.x * (1f / scaling), vector.y * (1f / scaling), vector.z * (1f / scaling));
        return outVector;
    }
}
