using System;
using System.Collections.Generic;
using System.Linq;
using Test.Octree;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Marching : MonoBehaviour
{
    [Header("Planet Settings")]
    public float isoLevel = 0.5f;

    [Header("Octree Settings")]
    public int rootNodeSize = 5;
    public int maxDepth = 8;
    [Range(1, 6)]
    public int standardDepth = 5;

    [Header("Auto Update")]
    public bool autoUpdate;

    [Header("Extra Settings")]
    public ShapeSettings shapeSettings;
    public ColourSettings colourSettings;

    private Mesh myMesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    ShapeGenerator shapeGenerator = new ShapeGenerator();
    ColourGenerator colourGenerator = new ColourGenerator();

    [HideInInspector]
    public bool shapeSettingsFoldout, colourSettingsFoldout;

    private Vector3 hitPoint;
    public Dictionary<Vector3, OctreeNode2> allNodes = new Dictionary<Vector3, OctreeNode2>();
    public Dictionary<Vector3, OctreeNode2> leafNodes = new Dictionary<Vector3, OctreeNode2>();

    private OctreeNode2 rootNode;

    void Start()
    {
        rootNode = new OctreeNode2(transform.position, rootNodeSize, 0, standardDepth, rootNode);

        shapeGenerator.UpdateSettings(shapeSettings);
        colourGenerator.UpdateSettings(colourSettings);

        allNodes = rootNode.TraverseOctree();
        foreach(var n in allNodes)
        {
            if(n.Value.leafNode)
            {
                leafNodes.Add(n.Value.nodePosition, n.Value);
            }
        }

        foreach(var n in leafNodes)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = n.Value.GetCorners()[i];
                float noise = shapeGenerator.CalculateUnscaledElevation(corner.normalized);
                n.Value.cornerValues[i] = n.Value.EvaluateScalarField(corner, shapeSettings.planetRadius, transform.position) + noise;
            }
            SetNeighbourNodes(n.Value);
        }

        myMesh = GenerateMesh();
        GetComponent<MeshFilter>().mesh = myMesh;
        GetComponent<MeshCollider>().sharedMesh = myMesh;
    }

    private void SetNeighbourNodes(OctreeNode2 n)
    {
        if(n.leafNode)
        {
            for (int i = 0; i < 26; i++)
            {
                if (allNodes.ContainsKey(n.nodePosition + n.GetNeighbourPositions()[i]))
                    n.nodeNeighbour.Add(allNodes[n.nodePosition + n.GetNeighbourPositions()[i]]);
                else if (allNodes.ContainsKey(n.parent.nodePosition + n.GetNeighbourPositions()[i]))
                    n.nodeNeighbour.Add(allNodes[n.parent.nodePosition + n.GetNeighbourPositions()[i]]);
            }
        }
        else
            for(int i = 0; i < 8; i++)
                SetNeighbourNodes(n.nodeChildren[i]);
    }

    Mesh GenerateMesh()
    {
        if (myMesh == null)
            myMesh = new Mesh();

        vertices.Clear();
        triangles.Clear();

        foreach (OctreeNode2 n in leafNodes.Values)
        {
            if (n.leafNode)
            {
                n.nodeVertices.Clear();
                n.nodeTriangles.Clear();
                for(int i = 0; i < 8; i++)
                {
                    if(Vector3.Distance(transform.position, n.GetCorners()[i]) < shapeSettings.planetRadius)
                    {
                        MarchCube(n, isoLevel, vertices, triangles);
                        break;
                    }
                }
            }
        }

        myMesh.Clear();
        myMesh.vertices = vertices.ToArray();
        myMesh.triangles = triangles.ToArray();

        myMesh.RecalculateNormals();
        return myMesh;
    }

    void MarchCube(OctreeNode2 node, float isoLevel, List<Vector3> vertices, List<int> triangles)
    {
        int cubeIndex = 0;
        float[] cubeCorners = new float[8];

        for (int i = 0; i < 8; i++)
        {
            cubeCorners[i] = node.CornerValues()[i];

            float epsilon = 0.0001f;
            if (cubeCorners[i] < isoLevel - epsilon)
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

            Vector3 vert1 = InterpolateVertex(edgeIndex1, cubeCorners, node);
            Vector3 vert2 = InterpolateVertex(edgeIndex2, cubeCorners, node);
            Vector3 vert3 = InterpolateVertex(edgeIndex3, cubeCorners, node);

            int vertexCount = vertices.Count;
                vertices.Add(vert1);
                vertices.Add(vert2);
                vertices.Add(vert3);

            triangles.Add(vertexCount);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);

            node.nodeVertices.Add(vert1);
            node.nodeVertices.Add(vert2);
            node.nodeVertices.Add(vert3);
            node.nodeTriangles.Add(new Vector3(vertexCount, vertexCount + 1, vertexCount + 2));
        }
    }

    private Vector3 InterpolateVertex(int edgeIndex, float[] cubeCorners, OctreeNode2 node)
    {
        Vector3 localP1 = MarchingTable.Edges[edgeIndex, 0];
        Vector3 localP2 = MarchingTable.Edges[edgeIndex, 1];

        Vector3 worldP1 = node.nodePosition + (localP1 - Vector3.one * 0.5f) * node.nodeSize;
        Vector3 worldP2 = node.nodePosition + (localP2 - Vector3.one * 0.5f) * node.nodeSize;

        int cornerIndex1 = GetCornerIndex(localP1);
        int cornerIndex2 = GetCornerIndex(localP2);

        if (cornerIndex1 < 0 || cornerIndex1 >= cubeCorners.Length ||
            cornerIndex2 < 0 || cornerIndex2 >= cubeCorners.Length)
        {
            return worldP1;
        }

        float val1 = cubeCorners[cornerIndex1];
        float val2 = cubeCorners[cornerIndex2];

        if (Mathf.Approximately(val1, val2))
        {
            return worldP1;
        }

        float t = (isoLevel - val1) / (val2 - val1);
        return Vector3.Lerp(worldP1, worldP2, t);
    }

    int GetCornerIndex(Vector3 localPosition)
    {
        for (int i = 0; i < MarchingTable.Corners.Length; i++)
        {
            if (MarchingTable.Corners[i] == new Vector3Int((int)localPosition.x, (int)localPosition.y, (int)localPosition.z))
            {
                return i;
            }
        }
        Debug.LogError($"Corner not found for position: {localPosition}");
        return -1; 
    }

    public void AddTerrain(Vector3 position, float strength)
    {
        OctreeNode2 foundNode = rootNode.FindNodeContainingPoint(rootNode, position);

        if (foundNode != null && foundNode.leafNode)
        {
            Vector3[] corners = foundNode.GetCorners();

            for (int i = 0; i < 8; i++)
            {
                float newValue = Mathf.Clamp01(foundNode.cornerValues[i] + strength);

                foundNode.cornerValues[i] = newValue;

                UpdateSharedCornerValues(foundNode, corners[i], newValue);
            }

            UpdateMesh();
        }
    }

    private void UpdateSharedCornerValues(OctreeNode2 n, Vector3 cornerPosition, float newValue)
    {
        foreach(OctreeNode2 neighbour in n.nodeNeighbour)
        {
            Vector3[] corners = neighbour.GetCorners();

            for (int i = 0; i < 8; i++)
            {
                if(Vector3.Distance(corners[i], cornerPosition) < Mathf.Epsilon)
                {
                    neighbour.cornerValues[i] = newValue;
                }
            }
        }
    }

    public void RemoveTerrain(Vector3 position, float strength)
    {
        OctreeNode2 foundNode = rootNode.FindNodeContainingPoint(rootNode, position);

        if (foundNode != null && foundNode.leafNode)
        {
            Vector3[] corners = foundNode.GetCorners();

            for (int i = 0; i < 8; i++)
            {
                float newValue = Mathf.Clamp01(foundNode.cornerValues[i] - strength);

                foundNode.cornerValues[i] = newValue;

                UpdateSharedCornerValues(foundNode, corners[i], newValue);
            }

            UpdateMesh();
        }
    }

    public void UpdateMesh()
    {
        myMesh = GenerateMesh();
        colourGenerator.UpdateElevation(shapeGenerator.elevationMinMax);
        GenerateColours();
        GetComponent<MeshFilter>().mesh = myMesh;
        GetComponent<MeshCollider>().sharedMesh = myMesh;
    }

    public void OnShapeSettingsUpdated()
    {
        if (autoUpdate)
        {
            UpdateMesh();
        }
    }

    public void onColourSettingsUpdated()
    {
        if (autoUpdate)
        {
            UpdateMesh();
            GenerateColours();
        }
    }

    void GenerateColours()
    {
        colourGenerator.UpdateColours();
        UpdateUVs(colourGenerator);
    }

    public void UpdateUVs(ColourGenerator colourGenerator)
    {
        Vector3[] vertices = myMesh.vertices;
        Vector2[] uv = new Vector2[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            Vector3 pointOnUnitSphere = vertex.normalized;

            Vector3 thisPosition = transform.position + new Vector3(shapeSettings.planetRadius, shapeSettings.planetRadius, shapeSettings.planetRadius);
            float distance = Vector3.Distance(thisPosition, vertex) - shapeSettings.planetRadius;
            float v = Mathf.Clamp01(distance);

            float biomeIndex = colourGenerator.BiomePercentFromPoint(pointOnUnitSphere);

            uv[i].x = biomeIndex;
        }

        myMesh.uv = uv;
    }
}
