using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MarchingCubes : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 32;
    public int height = 32;
    public int depth = 32;
    public float isoLevel = 0.5f;

    [Header("Planet Settings")]
    public float radius = 8f;

    [Header("Noise Settings")]
    public float noiseScale = 0.1f;
    public float noiseStrength = 0.2f; // Lower noise strength to preserve the sphere shape

    private float[,,] scalarField;

    private MeshCollider meshCollider;

    void Start()
    {
        meshCollider = GetComponent<MeshCollider>();
        GenerateScalarField();
        Mesh mesh = GenerateMesh();
        meshCollider.sharedMesh = mesh;
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void GenerateScalarField()
    {
        scalarField = new float[width + 1, height + 1, depth + 1];

        for (int x = 0; x <= width; x++)
        {
            for (int y = 0; y <= height; y++)
            {
                for (int z = 0; z <= depth; z++)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    float distanceFromCenter = Vector3.Distance(pos, new Vector3(width / 2f, height / 2f, depth / 2f));
                    float sphereValue = (radius - distanceFromCenter) / radius;
                    float noiseValue = Mathf.PerlinNoise(x * noiseScale, z * noiseScale) * noiseStrength;
                    scalarField[x, y, z] = sphereValue + noiseValue;
                }
            }
        }
    }

    public void UpdateTerrain(Vector3 hP)
    {
        // Convert position to integer coordinates
        int x = Mathf.RoundToInt(hP.x);
        int y = Mathf.RoundToInt(hP.y);
        int z = Mathf.RoundToInt(hP.z);

        // Ensure coordinates are within bounds
        x = Mathf.Clamp(x, 0, width);
        y = Mathf.Clamp(y, 0, height);
        z = Mathf.Clamp(z, 0, depth);

        // Update the scalar field at the specified position
        scalarField[x, y, z] = 1f;

        // Generate the mesh with the updated scalar field
        Mesh mesh = GenerateMesh();
        meshCollider.sharedMesh = mesh;
        GetComponent<MeshFilter>().mesh = mesh;
    }

    Mesh GenerateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    MarchCube(x, y, z, vertices, triangles);
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    void MarchCube(int x, int y, int z, List<Vector3> vertices, List<int> triangles)
    {
        int cubeIndex = 0;
        float[] cubeCorners = new float[8];

        for (int i = 0; i < 8; i++)
        {
            Vector3Int corner = MarchingTable.Corners[i];
            cubeCorners[i] = scalarField[x + corner.x, y + corner.y, z + corner.z];
            if (cubeCorners[i] < isoLevel)
            {
                cubeIndex |= 1 << i;
            }
        }

        if (cubeIndex == 0 || cubeIndex == 255) return;

        for (int i = 0; MarchingTable.Triangles[cubeIndex, i] != -1; i += 3)
        {
            int edgeIndex1 = MarchingTable.Triangles[cubeIndex, i];
            int edgeIndex2 = MarchingTable.Triangles[cubeIndex, i + 1];
            int edgeIndex3 = MarchingTable.Triangles[cubeIndex, i + 2];

            Vector3 vert1 = InterpolateVertex(edgeIndex1, x, y, z, cubeCorners);
            Vector3 vert2 = InterpolateVertex(edgeIndex2, x, y, z, cubeCorners);
            Vector3 vert3 = InterpolateVertex(edgeIndex3, x, y, z, cubeCorners);

            vertices.Add(vert1);
            vertices.Add(vert2);
            vertices.Add(vert3);

            int vertexCount = vertices.Count;
            triangles.Add(vertexCount - 3);
            triangles.Add(vertexCount - 2);
            triangles.Add(vertexCount - 1);
        }
    }

    Vector3 InterpolateVertex(int edgeIndex, int x, int y, int z, float[] cubeCorners)
    {
        Vector3 p1 = MarchingTable.Edges[edgeIndex, 0];
        Vector3 p2 = MarchingTable.Edges[edgeIndex, 1];

        float val1 = cubeCorners[MarchingTable.Corners.ToList().IndexOf(new Vector3Int((int)p1.x, (int)p1.y, (int)p1.z))];
        float val2 = cubeCorners[MarchingTable.Corners.ToList().IndexOf(new Vector3Int((int)p2.x, (int)p2.y, (int)p2.z))];

        float t = (isoLevel - val1) / (val2 - val1);
        return new Vector3(x, y, z) + p1 + t * (p2 - p1);
    }

    private void OnDrawGizmosSelected()
    {
        //for (int x = 0; x <= width; x++)
        //{
        //    for (int y = 0; y <= height; y++)
        //    {
        //        for (int z = 0; z <= depth; z++)
        //        {
        //            Gizmos.color = Color.red;
        //            Gizmos.DrawSphere(new Vector3(x, y, z), 0.1f);
        //        }
        //    }
        //}
    }
}