using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


namespace Test.Octree
{
    public class CreateOctree : MonoBehaviour
    {
        [SerializeField]
        public int maxDepth;
        [SerializeField]
        private int rootNodeSize;

        [SerializeField]
        private int radius = 2;

        [SerializeField]
        [Range(1, 20)]
        private int subdivisions = 3;


        //public List<OctreeNode> allNodes = new List<OctreeNode>();
        private Dictionary<Vector3, OctreeNode> allNodes = new Dictionary<Vector3, OctreeNode>();
        [SerializeField]
        List<int> triangles = new List<int>();
        [SerializeField]
        List<Vector3> vertices = new List<Vector3>();

        public float test2 = 0.93f;
        public float test3 = 0.86f;

        public OctreeNode rootNode;

        [SerializeField]
        private Vector3 selectNode;

        private Mesh mesh;
        public MeshFilter[] meshFilters = new MeshFilter[8];
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;

        private void Start()
        {
            meshFilter = GetComponent<MeshFilter>();
            rootNode = new OctreeNode(transform.position, rootNodeSize,0, maxDepth, rootNode);

            // Create an icosphere with 3 subdivisions
            // Icosphere icosphere = new Icosphere(subdivisions, this.transform.position, radius);
            //
            //// // Get the vertices and triangles
            //vertices = icosphere.GetVertices();
            //triangles = icosphere.GetTriangles();
            //// 
            //foreach(Vector3 v in vertices)
            //{
            //    rootNode.PlaceVertexInNode(rootNode, v);
            //}
            // 
            // AssignTrianglesToNodes();
            // 
            // for(int i = 0; i < rootNode.nodeChildren.Count; i++)
            // {
            //     GameObject me = new GameObject("mesh");
            //     me.transform.parent = this.transform;
            //     //me.transform.position = node.nodePosition;
            //     meshFilters[i] = me.AddComponent<MeshFilter>();
            // 
            //     List<int> nodeTriangles = rootNode.nodeChildren[i].GetAllTrianglesFromChildren();
            //     List<Vector3> nodeVertices = rootNode.nodeChildren[i].GetAllVoxelPointsFromChildren();
            // 
            //     rootNode.nodeChildren[i].allVertices = nodeVertices;
            //     rootNode.nodeChildren[i].allTriangles = nodeTriangles;
            // 
            //     for(int j = 0; j < nodeTriangles.Count; j++)
            //     {
            //         Debug.Log(nodeTriangles[j]);
            //     }
            // }
            // 
            // CreateMeshesForNodes();

            // Traverse the octree to collect all nodes and vertices
            allNodes = rootNode.TraverseNodesWithPositions();
            rootNode.AddNodeListToRoot(allNodes);

            // Assign depth values to nodes based on distance from the sphere's center
            rootNode.AssignDepthValues(this.transform.position, radius, maxDepth);


            foreach (var node in allNodes)
            {
                if (node.Value.onSurface)
                {
                    vertices.Add(node.Value.voxelPoint);
                }
            }

            //List<Vector3> sortedVerticesDescending = vertices.OrderBy(v => v.y).ToList();
            //
            //
            //int latitudeDivisions = 20; // Number of horizontal divisions
            //int longitudeDivisions = 20; // Number of vertical divisions
            //
            //
            //int numVertices = sortedVerticesDescending.Count;
            //
            //// Define triangles based on the existing vertices
            //List<int> triangles = new List<int>();
            //
            //// Assuming your vertices are ordered properly, you can create triangles as follows:
            //for (int i = 0; i < numVertices - 2; i++)
            //{
            //    triangles.Add(0); // The first vertex
            //    triangles.Add(i + 1); // The next vertex
            //    triangles.Add(i + 2); // The vertex after the next
            //}
            //
            //
            //mesh = new Mesh();
            //
            //mesh.vertices = sortedVerticesDescending.ToArray();
            //mesh.triangles = triangles.ToArray();
            //mesh.RecalculateBounds();
            //mesh.RecalculateNormals();
            //meshFilter.mesh = mesh;
            //
            //meshCollider = gameObject.AddComponent<MeshCollider>();
        }

        public void DrawHitcube(Vector3 hitPoint)
        {
            OctreeNode hitNode = rootNode.FindContainingNode(hitPoint);
            test = hitNode.nodePosition;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                rootNode.AssignDepthValues(this.transform.position, radius, maxDepth);
                //Debug.Log(allNodes[rootNode.FindContainingNode(selectNode).nodePosition].nodeNeighbours[23].nodePosition);
                //int tempCount = 0;
                //foreach (var n in allNodes)
                //{
                //    if(n.Value.leafNode) 
                //        tempCount++;
                //}
                //Debug.Log("Count: " + tempCount);
            }
        }

       // private void CreateMeshesForNodes()
       // {
       //     for (int i = 0; i < meshFilters.Length; i++)
       //     {
       //         OctreeNode node = rootNode.nodeChildren[i];
       //         if (node != null && node.allTriangles.Count > 0)
       //         {
       //             Mesh mesh = new Mesh();
       //
       //             mesh.vertices = node.allVertices.ToArray();
       //             mesh.triangles = node.allTriangles.ToArray();
       //
       //             // Recalculate normals and bounds for proper rendering
       //             mesh.RecalculateNormals();
       //             mesh.RecalculateBounds();
       //
       //             // Assign the mesh to the mesh filter
       //             meshFilters[i].mesh = mesh;
       //         }
       //     }
       // }

        private void AssignTrianglesToNodes()
        {
            // Traverse through the list of triangles
            for (int i = 0; i < triangles.Count; i += 3)
            {
                int v1 = triangles[i];
                int v2 = triangles[i + 1];
                int v3 = triangles[i + 2];

                // Calculate the triangle's center position
                Vector3 center = (vertices[v1] + vertices[v2] + vertices[v3]) / 3f;

                // Find the node that contains the triangle's center
                OctreeNode node = rootNode.FindContainingNode(center);

                // Store the triangle in the node
                node.nodeTriangle[0] = v1;
                node.nodeTriangle[1] = v2;
                node.nodeTriangle[2] = v3;
            }
        }

        private Vector3 test;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(gameObject.transform.position, radius);

            //Gizmos.color = Color.red;
            //Gizmos.DrawWireCube(rootNode.FindContainingNode(selectNode).nodePosition, new Vector3(rootNode.FindContainingNode(selectNode).nodeSize, rootNode.FindContainingNode(selectNode).nodeSize, rootNode.FindContainingNode(selectNode).nodeSize));
            //Debug.Log(rootNode.FindContainingNode(selectNode).nodePosition);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(rootNode.nodePosition, new Vector3(rootNode.nodeSize, rootNode.nodeSize, rootNode.nodeSize));

            foreach (var node in allNodes)
            {
                //if(node.Value.depthValue == 1)
                //{
                //    Gizmos.color = Color.blue;
                //    Gizmos.DrawWireCube(node.Value.nodePosition, new Vector3(node.Value.nodeSize, node.Value.nodeSize, node.Value.nodeSize));
                //    foreach(OctreeNode neighbour in node.Value.nodeNeighbours)
                //    {
                //        if(neighbour.depthValue == 0)
                //        {
                //            Gizmos.color = Color.red;
                //            Gizmos.DrawWireCube(neighbour.nodePosition, new Vector3(neighbour.nodeSize / 2, neighbour.nodeSize / 2, neighbour.nodeSize / 2));
                //        }
                //    }
                //}
                //if(node.Value.leafNode)
                //{
                //  Gizmos.color = Color.white;
                //  Gizmos.DrawWireCube(node.Key, new Vector3(node.Value.nodeSize, node.Value.nodeSize, node.Value.nodeSize));
                //}

               if(node.Value.onSurface) //node.Value.depthValue < test2 && node.Value.depthValue > test3
               {
                   //Gizmos.color = Color.white;
                   //Gizmos.DrawWireCube(node.Key, new Vector3(node.Value.nodeSize, node.Value.nodeSize, node.Value.nodeSize));
               
                   Gizmos.color = Color.red;
                   Gizmos.DrawSphere(node.Value.voxelPoint, .01f);
               }
            }
           //foreach(Vector3 v in vertices)
           //{
           //    Gizmos.color = Color.red;
           //    Gizmos.DrawSphere(v, .01f);
           //}
           //if (allNodes.ContainsKey(selectNode))
           //{
           //            Gizmos.color = Color.blue;
           //            Gizmos.DrawWireCube(allNodes[selectNode].nodePosition, new Vector3(allNodes[selectNode].nodeSize, allNodes[selectNode].nodeSize, allNodes[selectNode].nodeSize));
           //    foreach (Vector3 pos in allNodes[selectNode].nodeNeighboursPos)
           //    {
           //        Gizmos.color = Color.red;
           //        Gizmos.DrawWireCube(pos, new Vector3(allNodes[selectNode].nodeSize, allNodes[selectNode].nodeSize, allNodes[selectNode].nodeSize)); ;
           //    }
           //}
            //if (allNodes.ContainsKey(selectNode))
            //{
            //    foreach(OctreeNode neighbour in allNodes[selectNode].nodeNeigbours)
            //    {
            //        Gizmos.color = Color.blue;
            //        Gizmos.DrawWireCube(allNodes[selectNode].nodePosition, new Vector3(allNodes[selectNode].nodeSize, allNodes[selectNode].nodeSize, allNodes[selectNode].nodeSize));
            //        Gizmos.color = Color.red;
            //        Gizmos.DrawWireCube(neighbour.nodePosition, new Vector3(neighbour.nodeSize - .005f, neighbour.nodeSize - .005f, neighbour.nodeSize - .005f)); ;
            //    }
            //}
        }
    }
}
