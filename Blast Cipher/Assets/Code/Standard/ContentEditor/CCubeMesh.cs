using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCubeMesh
{
    private CQuadMesh[] quads = {        //Front, Back, Left, Right, Up, Down
        new CQuadMesh(new Vector3[] // Front
        {
            vertices[1],
            vertices[3],
            vertices[5],
            vertices[7]
        },
        new int[]{3,2,0,
                  0,1,3,}),
        new CQuadMesh(new Vector3[] //Back
        {
            vertices[0],
            vertices[2],
            vertices[4],
            vertices[6]
        },
        new int[]{1,0,3,
                  0,2,3,}),
        new CQuadMesh(new Vector3[] //Left
        {
            vertices[2],
            vertices[3],
            vertices[6],
            vertices[7]
        },
        new int[]{0,2,1,
                  1,2,3,}),
        new CQuadMesh(new Vector3[] // Right
        {
            vertices[0],
            vertices[1],
            vertices[4],
            vertices[5]
        },
        new int[]{1,2,0,
                  1,3,2,}),
        new CQuadMesh(new Vector3[] //UP
        {
            vertices[4],
            vertices[5],
            vertices[6],
            vertices[7]
        },
        new int[]{0,1,2,
                  3,2,1,}),
        new CQuadMesh(new Vector3[] //Down
        {
            vertices[0],
            vertices[1],
            vertices[2],
            vertices[3]
        },
        new int[]{2,1,0,
                  1,2,3,})
    };
    private static Vector3[] vertices = {
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

    public CCubeMesh(Vector3 position)
    {
        this.position = position;
        foreach (var quad in quads)
        {
            quad.Position = position;
        }
    }

    public Vector3[] Vertices
    {
        get
        {
            Vector3[] verts = new Vector3[8];

            for (int pos = 0; pos < vertices.Length; pos++)
            {
                verts[pos] = vertices[pos] + position;
            }
            return verts;
        }
    }
    public int[] Triangles
    {
        get
        {
            int[] tris = new int[36];
            int pos = 0;
            foreach (var quad in quads)
            {
                tris[pos] = GetVertexPosition(quad.Vertices[quad.Triangles[0]]);
                tris[pos + 1] = GetVertexPosition(quad.Vertices[quad.Triangles[1]]);
                tris[pos + 2] = GetVertexPosition(quad.Vertices[quad.Triangles[2]]);
                tris[pos + 3] = GetVertexPosition(quad.Vertices[quad.Triangles[3]]);
                tris[pos + 4] = GetVertexPosition(quad.Vertices[quad.Triangles[4]]);
                tris[pos + 5] = GetVertexPosition(quad.Vertices[quad.Triangles[5]]);
                pos += 6;
            }
            return tris;
        }
    }

    private int GetVertexPosition(Vector3 vertex)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertex.Equals(vertices[i]))
                return i;
        }
        return 0;
    }
}
