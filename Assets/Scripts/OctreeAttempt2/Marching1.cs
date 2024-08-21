//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Test.Octree;
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.UIElements;
//
//public class Marching : MonoBehaviour
//{
//    [Header("Planet Settings")]
//    public float radius = 1f;
//    public float isoLevel = 0.5f;
//
//    [Header("Noise Settings")]
//    public float noiseScale = 0.1f;
//    public float noiseStrength = 0.2f;
//
//    [Header("Octree Settings")]
//    public int rootNodeSize = 5;
//    public int maxDepth = 8;
//    public int standardDepth = 5;
//
//    List<Vector3> allVertices = new List<Vector3>();
//    List<int> allTriangles = new List<int>();
//
//    private Vector3 hitPoint;
//    public Dictionary<Vector3, OctreeNode2> allNodes = new Dictionary<Vector3, OctreeNode2>();
//
//    private OctreeNode2 rootNode;
//
//    void Start()
//    {
//        rootNode = new OctreeNode2(transform.position, rootNodeSize, 0, standardDepth, rootNode);
//        //rootNode.AssignScalarValues(rootNode, radius, transform.position);
//
//        allNodes = rootNode.TraverseOctree();
//        foreach(var n in allNodes)
//        {
//            for (int i = 0; i < 8; i++)
//                n.Value.cornerValues[i] = n.Value.EvaluateScalarField(n.Value.GetCorners()[i], radius, transform.position);//, noiseScale, noiseStrength);
//
//            SetNeighbourNodes(n.Value);
//        }
//
//        Mesh mesh = GenerateMesh();
//        GetComponent<MeshFilter>().mesh = mesh;
//        GetComponent<MeshCollider>().sharedMesh = mesh;
//    }
//
//    private void SetNeighbourNodes(OctreeNode2 n)
//    {
//        if(n.leafNode)
//        {
//            for (int i = 0; i < 26; i++)
//            {
//                if (allNodes.ContainsKey(n.nodePosition + n.GetNeighbourPositions()[i]))
//                    n.nodeNeighbour.Add(allNodes[n.nodePosition + n.GetNeighbourPositions()[i]]);
//                else if (allNodes.ContainsKey(n.parent.nodePosition + n.GetNeighbourPositions()[i]))
//                    n.nodeNeighbour.Add(allNodes[n.parent.nodePosition + n.GetNeighbourPositions()[i]]);
//            }
//        }
//        else
//            for(int i = 0; i < 8; i++)
//                SetNeighbourNodes(n.nodeChildren[i]);
//    }
//
//    Mesh GenerateMesh()
//    {
//        List<Vector3> vertices = new List<Vector3>();
//        List<int> triangles = new List<int>();
//        Debug.Log("Allnodes Counted: " + allNodes.Count);
//        foreach (OctreeNode2 n in allNodes.Values)
//        {
//            if (n.leafNode)
//            {
//                MarchCube(n, isoLevel, vertices, triangles);
//            }
//        }
//
//        Mesh mesh = new Mesh
//        {
//            vertices = vertices.ToArray(),
//            triangles = triangles.ToArray()
//        };
//        mesh.RecalculateNormals();
//        allVertices = vertices;
//        allTriangles = triangles;
//        return mesh;
//    }
//
//
//    void MarchCube(OctreeNode2 node, float isoLevel, List<Vector3> vertices, List<int> triangles)
//    {
//        int cubeIndex = 0;
//        float[] cubeCorners = new float[8];
//
//        // Populate cubeCorners with scalar field values
//        for (int i = 0; i < 8; i++)
//        {
//            cubeCorners[i] = node.CornerValues()[i];
//
//            // Debugging precision issues
//            float epsilon = 0.0001f;
//            if (cubeCorners[i] < isoLevel - epsilon)
//            {
//                cubeIndex |= 1 << i;
//            }
//        }
//
//        if (cubeIndex == 0 || cubeIndex == 255) return;
//
//        // Generate triangles
//        for (int i = 0; MarchingTable.Triangles[cubeIndex, i] != -1; i += 3)
//        {
//            int edgeIndex1 = MarchingTable.Triangles[cubeIndex, i];
//            int edgeIndex2 = MarchingTable.Triangles[cubeIndex, i + 1];
//            int edgeIndex3 = MarchingTable.Triangles[cubeIndex, i + 2];
//
//            Vector3 vert1 = InterpolateVertex(edgeIndex1, cubeCorners, node);
//            Vector3 vert2 = InterpolateVertex(edgeIndex2, cubeCorners, node);
//            Vector3 vert3 = InterpolateVertex(edgeIndex3, cubeCorners, node);
//
//            int vertexCount = vertices.Count;
//                vertices.Add(vert1);
//                vertices.Add(vert2);
//                vertices.Add(vert3);
//            triangles.Add(vertexCount);
//            triangles.Add(vertexCount + 1);
//            triangles.Add(vertexCount + 2);
//
//            //node.nodeVertices.Add(vert1);
//            //node.nodeVertices.Add(vert2);
//            //node.nodeVertices.Add(vert3);
//            //node.nodeTriangles.Add(new Vector3(vertexCount, vertexCount + 1, vertexCount + 2));
//        }
//    }
//
//    private Vector3 InterpolateVertex(int edgeIndex, float[] cubeCorners, OctreeNode2 node)
//    {
//        Vector3 localP1 = MarchingTable.Edges[edgeIndex, 0];
//        Vector3 localP2 = MarchingTable.Edges[edgeIndex, 1];
//
//        Vector3 worldP1 = node.nodePosition + (localP1 - Vector3.one * 0.5f) * node.nodeSize;
//        Vector3 worldP2 = node.nodePosition + (localP2 - Vector3.one * 0.5f) * node.nodeSize;
//
//        int cornerIndex1 = GetCornerIndex(localP1);
//        int cornerIndex2 = GetCornerIndex(localP2);
//
//        if (cornerIndex1 < 0 || cornerIndex1 >= cubeCorners.Length ||
//            cornerIndex2 < 0 || cornerIndex2 >= cubeCorners.Length)
//        {
//            return worldP1;
//        }
//
//        float val1 = cubeCorners[cornerIndex1];
//        float val2 = cubeCorners[cornerIndex2];
//
//        if (Mathf.Approximately(val1, val2))
//        {
//            return worldP1;
//        }
//
//        float t = (isoLevel - val1) / (val2 - val1);
//        return Vector3.Lerp(worldP1, worldP2, t);
//    }
//
//    int GetCornerIndex(Vector3 localPosition)
//    {
//        // Convert the local position to the nearest corner index.
//        for (int i = 0; i < MarchingTable.Corners.Length; i++)
//        {
//            if (MarchingTable.Corners[i] == new Vector3Int((int)localPosition.x, (int)localPosition.y, (int)localPosition.z))
//            {
//                return i;
//            }
//        }
//        Debug.LogError($"Corner not found for position: {localPosition}");
//        return -1; // Invalid index
//    }
//
//    public void AddTerrain(Vector3 position, float strength)
//    {
//        OctreeNode2 foundNode = rootNode.FindNodeContainingPoint(rootNode, position);
//
//        if (foundNode != null && foundNode.leafNode)
//        {
//            Vector3[] corners = foundNode.GetCorners();
//
//            for (int i = 0; i < 8; i++)
//            {
//                // Calculate the new value
//                float newValue = Mathf.Clamp01(foundNode.cornerValues[i] + strength);
//
//                // Update the value in the current node
//                foundNode.cornerValues[i] = newValue;
//
//                // Update the value in all adjacent nodes sharing this corner
//                UpdateSharedCornerValues(foundNode, corners[i], newValue);
//            }
//
//            allNodes = rootNode.TraverseOctree();
//            UpdateMesh();
//        }
//    }
//
//    private void UpdateSharedCornerValues(OctreeNode2 n, Vector3 cornerPosition, float newValue)
//    {
//        Queue<OctreeNode2> nodesToCheck = new Queue<OctreeNode2>();
//        nodesToCheck.Enqueue(rootNode);
//
//        while (nodesToCheck.Count > 0)
//        {
//            OctreeNode2 currentNode = nodesToCheck.Dequeue();
//
//            if (currentNode.leafNode)
//            {
//                Vector3[] corners = currentNode.GetCorners();
//                for (int i = 0; i < 8; i++)
//                {
//                    if (Vector3.Distance(corners[i], cornerPosition) < Mathf.Epsilon)
//                    {
//                        currentNode.cornerValues[i] = newValue;
//                    }
//                }
//            }
//            else
//            {
//                foreach (var child in currentNode.nodeChildren)
//                {
//                    if (child != null)
//                    {
//                        nodesToCheck.Enqueue(child);
//                    }
//                }
//            }
//        }
//    }
//
//    public void RemoveTerrain(Vector3 position, float strength)
//    {
//        OctreeNode2 foundNode = rootNode.FindNodeContainingPoint(rootNode, position);
//
//        if (foundNode != null && foundNode.leafNode)
//        {
//            Vector3[] corners = foundNode.GetCorners();
//
//            for (int i = 0; i < 8; i++)
//            {
//                // Calculate the new value
//                float newValue = Mathf.Clamp01(foundNode.cornerValues[i] - strength);
//
//                // Update the value in the current node
//                foundNode.cornerValues[i] = newValue;
//
//                // Update the value in all adjacent nodes sharing this corner
//                UpdateSharedCornerValues(foundNode, corners[i], newValue);
//            }
//
//            allNodes = rootNode.TraverseOctree();
//            UpdateMesh();
//        }
//    }
//
//    public void UpdateMesh()
//    {
//        Mesh mesh = GenerateMesh();
//        GetComponent<MeshFilter>().mesh = mesh;
//        GetComponent<MeshCollider>().sharedMesh = mesh;
//    }
//
//    //public void CameraPos(Vector3 camPos)
//    //{
//    //    OctreeNode2 foundNode = rootNode.FindNodeContainingPoint(rootNode, camPos);
//    //
//    //    if (Vector3.Distance(foundNode.nodePosition, camPos) < 0.02f)
//    //    {
//    //        foundNode.SubdivideIfNeeded(maxDepth, foundNode.cornerValues, foundNode.GetCorners());
//    //    }
//    //    else
//    //        foundNode.Undivide();
//    //
//    //    //for (int i = 0; i < 8; i++)
//    //    //    foundNode.cornerValues[i] = foundNode.EvaluateScalarField(foundNode.GetCorners()[i], radius, transform.position, noiseScale, noiseStrength);
//    //    allNodes = rootNode.TraverseOctree();
//    //
//    //    UpdateMesh();
//    //
//    //}
//}
