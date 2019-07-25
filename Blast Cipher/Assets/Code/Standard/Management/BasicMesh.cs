using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMesh
{
    public List<Vector3> Vertices = new List<Vector3>();
    public List<int> Triangles = new List<int>();

    int vertexHarvest = 0;
    int triangleHarvest = 0;

    public void Sow()
    {
        vertexHarvest = 0;
        triangleHarvest = 0;
    }

    public bool Reap(out Vector3 vertex)
    {
        bool empty = vertexHarvest-1 >= Vertices.Count;
        vertex = Vector3.zero;
        if (!empty)
        {
            vertex = Vertices[vertexHarvest];
            vertexHarvest++;
        }
        return empty;
    }
    public bool Reap(out int triangle)
    {
        bool empty = triangleHarvest - 1 >= Triangles.Count;
        triangle = 0;
        if (!empty)
        {
            triangle = Triangles[triangleHarvest];
            triangleHarvest++;
        }
        return empty;
    }
}
