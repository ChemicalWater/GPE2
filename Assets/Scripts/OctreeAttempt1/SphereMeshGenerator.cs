using System.Collections.Generic;
using UnityEngine;

public class SphereMeshGenerator : MonoBehaviour
{
    public int numVertices = 100; // Adjust as needed
    public float radius = 1.0f; // Sphere radius

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    void Start()
    {
        GenerateSphereVertices();
        CreateSphereMesh();
    }

    void GenerateSphereVertices()
    {
        // Generate vertices for a sphere
        for (int lat = 0; lat <= numVertices; lat++)
        {
            for (int lon = 0; lon <= numVertices; lon++)
            {
                float theta = (2 * Mathf.PI * lon) / numVertices;
                float phi = (Mathf.PI * lat) / numVertices;

                float x = radius * Mathf.Sin(phi) * Mathf.Cos(theta);
                float y = radius * Mathf.Sin(phi) * Mathf.Sin(theta);
                float z = radius * Mathf.Cos(phi);

                vertices.Add(new Vector3(x, y, z));
            }
        }
    }

    void CreateSphereMesh()
    {
        int verticesPerRow = numVertices + 1;

        // Create triangles to form the sphere
        for (int lat = 0; lat < numVertices; lat++)
        {
            for (int lon = 0; lon < numVertices; lon++)
            {
                int currentVertex = lat * verticesPerRow + lon;

                // Define the indices to create triangles
                triangles.Add(currentVertex);
                triangles.Add(currentVertex + 1);
                triangles.Add(currentVertex + verticesPerRow);

                triangles.Add(currentVertex + verticesPerRow);
                triangles.Add(currentVertex + 1);
                triangles.Add(currentVertex + verticesPerRow + 1);
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        // Attach the mesh to a GameObject or perform further processing.
        GameObject sphere = new GameObject("SphereMesh");
        sphere.AddComponent<MeshFilter>().mesh = mesh;
        sphere.AddComponent<MeshRenderer>();
    }
}
