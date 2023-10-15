using System.Collections.Generic;
using UnityEngine;

public class Icosphere
{
    private List<Vector3> vertices;
    private List<int> triangles;

    public Icosphere(int subdivisions, Vector3 centerPoint, float radius)
    {
        // Create an icosahedron
        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        vertices = new List<Vector3>
        {
            new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
            new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
            new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1)
        };

        triangles = new List<int>
        {
            0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
            1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
            3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
            4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1
        };

        // Subdivide the icosahedron
        for (int i = 0; i < subdivisions; i++)
        {
            Subdivide();
        }

        // Center and scale the vertices
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = vertices[i].normalized * radius + centerPoint;
        }
    }

    // Subdivide the triangles of the icosphere
    private void Subdivide()
    {
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();

        for (int i = 0; i < triangles.Count; i += 3)
        {
            int v1 = triangles[i];
            int v2 = triangles[i + 1];
            int v3 = triangles[i + 2];

            Vector3 midPoint1 = (vertices[v1] + vertices[v2]).normalized;
            Vector3 midPoint2 = (vertices[v2] + vertices[v3]).normalized;
            Vector3 midPoint3 = (vertices[v3] + vertices[v1]).normalized;

            newVertices.Add(vertices[v1]);
            newVertices.Add(midPoint1);
            newVertices.Add(midPoint3);

            newVertices.Add(midPoint1);
            newVertices.Add(vertices[v2]);
            newVertices.Add(midPoint2);

            newVertices.Add(midPoint2);
            newVertices.Add(vertices[v3]);
            newVertices.Add(midPoint3);

            newVertices.Add(midPoint1);
            newVertices.Add(midPoint2);
            newVertices.Add(midPoint3);

            int start = newVertices.Count - 12;

            newTriangles.Add(start);
            newTriangles.Add(start + 1);
            newTriangles.Add(start + 2);

            newTriangles.Add(start + 3);
            newTriangles.Add(start + 4);
            newTriangles.Add(start + 5);

            newTriangles.Add(start + 6);
            newTriangles.Add(start + 7);
            newTriangles.Add(start + 8);

            newTriangles.Add(start + 9);
            newTriangles.Add(start + 10);
            newTriangles.Add(start + 11);
        }

        vertices = newVertices;
        triangles = newTriangles;
    }

    // Get the generated vertices and triangles
    public List<Vector3> GetVertices() => vertices;
    public List<int> GetTriangles() => triangles;
}
