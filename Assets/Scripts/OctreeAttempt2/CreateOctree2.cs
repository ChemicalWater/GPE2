using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


namespace Test.Octree
{
    public class CreateOctree2 : MonoBehaviour
    {

        [SerializeField]
        Camera cam;

        [SerializeField]
        public int maxDepth;

        [SerializeField]
        public int standardDepth;

        [SerializeField]
        private int rootNodeSize;

        [SerializeField]
        private int radius = 2;

        public List<OctreeNode2> allNodes = new List<OctreeNode2>();

        public OctreeNode2 rootNode;

        [SerializeField]
        private Vector3 placeVertex;

        public List<Vector3> vertices = new List<Vector3>();
        public List<int> triangles = new List<int>();
        private List<Vector3> triangleSets = new List<Vector3>();

        private MeshFilter meshFilter;

        private void Start()
        {
            meshFilter = GetComponent<MeshFilter>();

            rootNode = new OctreeNode2(transform.position, rootNodeSize,0, standardDepth, rootNode);

            //rootNode.AssignScalarValues(rootNode, radius, transform.position);

            //allNodes = rootNode.TraverseOctree();

            foreach (OctreeNode2 node in allNodes)
            {
                if(node.leafNode)
                {
                    vertices.Add(node.voxelPoint);

                }
            }

            CreateTriangles();
        }

        private void GenerateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            meshFilter.mesh = mesh;
        }

        private void CreateTriangles()
        {

            triangles.Add(1);
            triangles.Add(2);
            triangles.Add(3);

            triangles.Add(1);
            triangles.Add(0);
            triangles.Add(2);

            triangles.Add(0);
            triangles.Add(4);
            triangles.Add(2);

            triangles.Add(4);
            triangles.Add(6);
            triangles.Add(2);

            triangles.Add(2);
            triangles.Add(6);
            triangles.Add(3);

            triangles.Add(3);
            triangles.Add(6);
            triangles.Add(7);

            triangles.Add(4);
            triangles.Add(5);
            triangles.Add(6);

            triangles.Add(5);
            triangles.Add(7);
            triangles.Add(6);

            triangles.Add(1);
            triangles.Add(3);
            triangles.Add(5);

            triangles.Add(5);
            triangles.Add(3);
            triangles.Add(7);

            triangles.Add(1);
            triangles.Add(4);
            triangles.Add(0);

            triangles.Add(5);
            triangles.Add(4);
            triangles.Add(1);


            List<Vector3> testVertices = new List<Vector3>();
            foreach (OctreeNode2 node in allNodes)
            {
                if (node.leafNode)
                {
                    // for (int i = 0; i < node.parent.nodeChildren.Length - 1; i++)
                    // {
                    //     int i1 = vertices.IndexOf(node.voxelPoint);
                    //     int i2 = vertices.IndexOf(node.parent.nodeChildren[i].voxelPoint);
                    //     int i3 = vertices.IndexOf(node.parent.nodeChildren[i + 1].voxelPoint);
                    //
                    //     Vector3[] triangleOptions = new Vector3[]
                    //     {
                    //         new Vector3(i1, i2, i3),
                    //         new Vector3(i1, i3, i2),
                    //         new Vector3(i2, i1, i3),
                    //         new Vector3(i2, i3, i1),
                    //         new Vector3(i3, i1, i2),
                    //         new Vector3(i3, i2, i1),
                    //     };
                    //
                    //     foreach(Vector3 v in triangleOptions)
                    //     {
                    //         if(triangleSets.Contains(v))
                    //         {
                    //             return;
                    //         }
                    //         triangleSets.Add(new Vector3(i1,i2,i3));
                    //         triangles.Add(i1);
                    //         triangles.Add(i2);
                    //         triangles.Add(i3);
                    //     }
                    // }
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(gameObject.transform.position, radius);

            //Gizmos.color = Color.red;
            //Gizmos.DrawWireCube(rootNode.FindContainingNode(selectNode).nodePosition, new Vector3(rootNode.FindContainingNode(selectNode).nodeSize, rootNode.FindContainingNode(selectNode).nodeSize, rootNode.FindContainingNode(selectNode).nodeSize));
            //Debug.Log(rootNode.FindContainingNode(selectNode).nodePosition);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(rootNode.nodePosition, new Vector3(rootNode.nodeSize, rootNode.nodeSize, rootNode.nodeSize));

            foreach(OctreeNode2 n in allNodes)
            {
                if (n.leafNode)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(n.voxelPoint, 0.1f);
                    for(int i = 0; i < n.GetCorners().Length; i++)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawWireCube(n.nodePosition, new Vector3(n.nodeSize, n.nodeSize, n.nodeSize));
                        Handles.Label(n.GetCorners()[i], n.CornerValues()[i].ToString());
                        Handles.Label(n.voxelPoint, vertices.IndexOf(n.voxelPoint).ToString());
                    }
                }
            }
        }
    }
}
