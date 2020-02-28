using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCubeMesh
{
    private readonly CQuadMesh[] allQuads = {        //Front, Back, Left, Right, Up, Down
        new CQuadMesh(new Vector3[] // Front
        {
            cubicVertices[1],
            cubicVertices[3],
            cubicVertices[5],
            cubicVertices[7]
        },
        new int[]{3,2,0,
                  0,1,3}),
        new CQuadMesh(new Vector3[] //Back
        {
            cubicVertices[0],
            cubicVertices[2],
            cubicVertices[4],
            cubicVertices[6]
        },
        new int[]{1,0,3,
                  0,2,3}),
        new CQuadMesh(new Vector3[] //Left
        {
            cubicVertices[2],
            cubicVertices[3],
            cubicVertices[6],
            cubicVertices[7]
        },
        new int[]{0,2,1,
                  1,2,3}),
        new CQuadMesh(new Vector3[] // Right
        {
            cubicVertices[0],
            cubicVertices[1],
            cubicVertices[4],
            cubicVertices[5]
        },
        new int[]{1,2,0,
                  1,3,2}),
        new CQuadMesh(new Vector3[] //UP
        {
            cubicVertices[4],
            cubicVertices[5],
            cubicVertices[6],
            cubicVertices[7]
        },
        new int[]{0,1,2,
                  3,2,1}),
        new CQuadMesh(new Vector3[] //Down
        {
            cubicVertices[0],
            cubicVertices[1],
            cubicVertices[2],
            cubicVertices[3]
        },
        new int[]{2,1,0,
                  1,2,3})
    };
    private static Vector3[] cubicVertices = {
            new Vector3(-0.5f,0,-0.5f),
            new Vector3(-0.5f,0,0.5f),
            new Vector3(0.5f,0,-0.5f),
            new Vector3(0.5f,0,0.5f),
            new Vector3(-0.5f,1f,-0.5f),
            new Vector3(-0.5f,1f,0.5f),
            new Vector3(0.5f,1f,-0.5f),
            new Vector3(0.5f,1f,0.5f),
        };
    private Vector3 position;

    public Vector3Int Position { get => Convert(position); }

    public CCubeMesh(Vector3 position)
    {
        this.position = position;
        foreach (var quad in allQuads)
        {
            quad.Position = position;
        }
    }

    public BasicMesh GetMeshData(bool front, bool back, bool left, bool right, bool up, bool down)
    {
        BasicMesh meshData = new BasicMesh();
        if (front)
        {
            ReapQuad(ref meshData,0);
        }
        if (back)
        {
            ReapQuad(ref meshData,1);
        }
        if (left)
        {
            ReapQuad(ref meshData,2);
        }
        if (right)
        {
            ReapQuad(ref meshData,3);
        }
        if (up)
        {
            ReapQuad(ref meshData,4);
        }
        if (down)
        {
            ReapQuad(ref meshData,5);
        }

        return meshData;
    }

    private void ReapQuad(ref BasicMesh meshData, int axisID)
    {
        int triOffset = meshData.Vertices.Count;
        foreach (var vert in allQuads[axisID].Vertices)
        {
            meshData.Vertices.Add(vert + allQuads[axisID].Position);
        }
        foreach (var tri in allQuads[axisID].Triangles)
        {
            meshData.Triangles.Add(tri + triOffset);
        }
    }

    private Vector3Int Convert(Vector3 vector)
    {
        return new Vector3Int((int)vector.x, (int)vector.y, (int)vector.z);
    }
}
