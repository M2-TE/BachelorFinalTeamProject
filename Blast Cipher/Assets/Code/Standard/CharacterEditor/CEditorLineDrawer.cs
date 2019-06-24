using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEditorLineDrawer : MonoBehaviour
{
    public struct CEditorLine
    {
        public Vector3 a;
        public Vector3 b;
        public Color color;

        public CEditorLine(Vector3 start, Vector3 end, Color color)
        {
            a = start;
            b = end;
            this.color = color;
        }
    }

    public Material material;
    internal static List<CEditorLine> lines = new List<CEditorLine>();
    internal static List<CEditorLine> wireframe = new List<CEditorLine>();

    public static void ClearLines() => lines.Clear();

    public static void ClearWireframe() => wireframe.Clear();

    private void OnPostRender()
    {
        GL.Begin(GL.LINES);
        material.SetPass(0);
        for(int i = 0; i < lines.Count; i++)
        {
            GL.Color(lines[i].color);
            GL.Vertex(lines[i].a);
            GL.Vertex(lines[i].b);
        }

        for(int i = 0; i < wireframe.Count; i++)
        {
            GL.Color(wireframe[i].color);
            GL.Vertex(wireframe[i].a);
            GL.Vertex(wireframe[i].b);
        }

        GL.End();
    }
}
