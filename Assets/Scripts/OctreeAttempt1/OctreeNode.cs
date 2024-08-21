using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

namespace Test.Octree
{
    public class OctreeNode
    {
        public Dictionary<Vector3, OctreeNode> nodeInfo { get; private set; }
        private OctreeNode rootNode;

        // Stuff for Mesh
        public List<Vector3> allVertices = new List<Vector3>();
        public List<int> allTriangles = new List<int>();

        private Mesh mesh;
        private MeshFilter meshFilter;

        public bool bigBoyNode = false;

        public Vector3 nodePosition;
        public float nodeSize;
        public float depthValue;
        public Vector3 voxelPoint;
        public OctreeNode[] nodeChildren = new OctreeNode[8];
        public OctreeNode[] nodeNeighbours = new OctreeNode[26];
        public Vector3[] nodeChildrenPos = new Vector3[8];
        public Vector3[] nodeNeighboursPos = new Vector3[26];
        private OctreeNode nodeParent;
        public int[] nodeTriangle = new int[3];
        public bool leafNode;
        public bool onSurface;
        public int nodeDepth { get; private set; }
        public int maxDepth { get; private set; }

        public OctreeNode (Vector3 nodePos, float size, int myDepth, int maxDepth, OctreeNode parent = null)
        {
            this.nodePosition = nodePos;
            this.nodeSize = size;
            this.nodeDepth = myDepth;
            this.maxDepth = maxDepth;
            this.nodeParent = parent;

            rootNode = GetRootNode(this);

            //GetNeighbourPositions();

            Subdivide();
        }

        public OctreeNode GetRootNode(OctreeNode node)
        {
            if (node.nodeParent != null)
            {
                return GetRootNode(node.nodeParent);
            }
            else
                return node;
        }

        public void AddNodeListToRoot(Dictionary<Vector3, OctreeNode> allNodes)
        {
            nodeInfo = allNodes;
        }

        public void GetChildPositions()
        {
            Vector3 halfSize = new Vector3(nodeSize * .25f, nodeSize * .25f, nodeSize * .25f);
            Vector3[] childOffsets = new Vector3[]
            {
            new Vector3(-halfSize.x, -halfSize.y, -halfSize.z), // Child 0 (bottom-left-back)
            new Vector3(halfSize.x, -halfSize.y, -halfSize.z),  // Child 1 (bottom-right-back)
            new Vector3(-halfSize.x, halfSize.y, -halfSize.z),  // Child 2 (top-left-back)
            new Vector3(halfSize.x, halfSize.y, -halfSize.z),   // Child 3 (top-right-back)
            new Vector3(-halfSize.x, -halfSize.y, halfSize.z),  // Child 4 (bottom-left-front)
            new Vector3(halfSize.x, -halfSize.y, halfSize.z),   // Child 5 (bottom-right-front)
            new Vector3(-halfSize.x, halfSize.y, halfSize.z),   // Child 6 (top-left-front)
            new Vector3(halfSize.x, halfSize.y, halfSize.z)     // Child 7 (top-right-front)
            };

            for (int i = 0; i < nodeChildrenPos.Length; i++)
            {
                nodeChildrenPos[i] = nodePosition + childOffsets[i];
            }
        }

        public void GetNeighbourPositions()
        {
            Vector3 mySize = new Vector3(nodeSize, nodeSize, nodeSize);
            Vector3[] neighbourOffsets = new Vector3[]
            {
                new Vector3(-mySize.x, -mySize.y, -mySize.z),     // Neighbour 0 (bottom-left-back)
                new Vector3(mySize.x, -mySize.y, -mySize.z),      // Neighbour 1 (bottom-right-back)
                new Vector3(-mySize.x, mySize.y, -mySize.z),      // Neighbour 2 (top-left-back)
                new Vector3(mySize.x, mySize.y, -mySize.z),       // Neighbour 3 (top-right-back)
                new Vector3(-mySize.x, -mySize.y, mySize.z),      // Neighbour 4 (bottom-left-front)
                new Vector3(mySize.x, -mySize.y, mySize.z),       // Neighbour 5 (bottom-right-front)
                new Vector3(-mySize.x, mySize.y, mySize.z),       // Neighbour 6 (top-left-front)
                new Vector3(mySize.x, mySize.y, mySize.z),        // Neighbour 7 (top-right-front)
                new Vector3(-mySize.x, -mySize.y, 0),             // Neighbour 8 (bottom-left-middle)
                new Vector3(mySize.x, -mySize.y, 0),              // Neighbour 9 (bottom-right-middle)
                new Vector3(-mySize.x, mySize.y, 0),              // Neighbour 10 (top-left-middle)
                new Vector3(mySize.x, mySize.y, 0),               // Neighbour 11 (top-right-middle)
                new Vector3(-mySize.x, 0, -mySize.z),             // Neighbour 12 (left-back-middle)
                new Vector3(mySize.x, 0, -mySize.z),              // Neighbour 13 (right-back-middle)
                new Vector3(-mySize.x, 0, mySize.z),              // Neighbour 14 (left-front-middle)
                new Vector3(mySize.x, 0, mySize.z),               // Neighbour 15 (right-front-middle)
                new Vector3(0, -mySize.y, -mySize.z),             // Neighbour 16 (bottom-back-middle)
                new Vector3(0, mySize.y, -mySize.z),              // Neighbour 17 (top-back-middle)
                new Vector3(0, -mySize.y, mySize.z),              // Neighbour 18 (bottom-front-middle)
                new Vector3(0, mySize.y, mySize.z),               // Neighbour 19 (top-front-middle)
                new Vector3(0, -mySize.y, 0),                     // Neighbour 20 (bottom-middle)
                new Vector3(0, mySize.y, 0),                      // Neighbour 21 (top-middle)
                new Vector3(mySize.x, 0, 0),                      // Neighbour 22 (right-middle)
                new Vector3(-mySize.x, 0, 0),                     // Neighbour 23 (left-middle)
                new Vector3(0, 0, -mySize.z),                     // Neighbour 24 (back-middle)
                new Vector3(0, 0, mySize.z)                      // Neighbour 25 (front-middle)
            };

            for (int i = 0; i < nodeNeighboursPos.Length; i++)
            {
                nodeNeighboursPos[i] = nodePosition + neighbourOffsets[i];
            }
            AddNeighbours();
            if(HaveChildren())
            {
                for (int i = 0; i < nodeChildren.Length; i++)
                {
                    nodeChildren[i].GetNeighbourPositions();
                }
            }
            
        }

        public bool HaveChildren()
        {
            if (nodeChildren[0] != null)
            {
                leafNode = false;
                    return true;
            }
            else
            {
                leafNode = true;
                return false;
            } 
        }

        private void AddNeighbours()
        {
            for(int i = 0; i < nodeNeighboursPos.Length ;i++)
            {
                if (rootNode.nodeInfo.ContainsKey(nodeNeighboursPos[i]))
                {
                    nodeNeighbours[i] = rootNode.nodeInfo[nodeNeighboursPos[i]];
                }
            }
        }

        // Function to place a vertex inside the octree
        public void PlaceVertexInNode(OctreeNode node, Vector3 vertex)
        {
            if (node.IsPointInsideNode(vertex))
            {
                //if (!node.HaveChildren())
                //{
                //    // If the node doesn't have children and the vertex is inside, then we need to subdivide this node.
                //    node.Subdivide();
                //}


                if(node.HaveChildren())
                {
                    // Traverse through the child nodes to find the correct node containing the vertex.
                    foreach (OctreeNode child in node.nodeChildren)
                    {
                        if (child.IsPointInsideNode(vertex))
                        {
                            // Recursively call the method on the child node that contains the vertex.
                            PlaceVertexInNode(child, vertex);
                            return; // Stop the loop as we found the correct child.
                        }
                    }
                } else
                {
                    node.voxelPoint = vertex;
                }
            } else
            {
                //PlaceVertexInNode(node, node.ClosestPointInsideNode(vertex));
            }

        }

        //Split this node into children
        public void Subdivide()
        {
            GetChildPositions();
            if (nodeDepth < maxDepth)
            {
                for(int i = 0; i < nodeChildrenPos.Length; i++)
                {
                    nodeChildren[i] = (new OctreeNode(nodeChildrenPos[i], (nodeSize * .5f), (nodeDepth+1) ,maxDepth, this));
                    nodeChildren[i].Subdivide();
                }
            }
            HaveChildren();
        }

        // combine this node
        public void Undivide()
        {
            if(HaveChildren())
            {
                Vector3 average;
            }
        }

        // Find the node that contains a specific point in the octree
        public OctreeNode FindContainingNode(Vector3 point)
        {
            // Start the recursive traversal from the root node
            return FindContainingNodeRecursively(rootNode, point);
        }

        // Recursive method to find the node containing the point
        private OctreeNode FindContainingNodeRecursively(OctreeNode node, Vector3 point)
        {
            // If the node is null, it means we've reached an empty space and haven't found a containing node
            if (node == null)
            {
                return null;
            }

            // If the point is inside the bounds of the current node, check its children
            if (node.IsPointInsideNode(point))
            {
                // If the node has no children, it means it's the smallest node containing the point
                if (!node.HaveChildren())
                {
                    return node;
                }

                // Recursively check the children nodes
                for (int i = 0; i < 8; i++)
                {
                    OctreeNode child = node.nodeChildren[i];
                    OctreeNode containingNode = FindContainingNodeRecursively(child, point);
                    if (containingNode != null)
                    {
                        return containingNode;
                    }
                }
            }

            // If the point is not inside the bounds of the current node or its children, return null
            return null;
        }

        // Find the closest point inside the node to a given vector
        public Vector3 ClosestPointInsideNode(Vector3 point)
        {
            Vector3 halfSize = new Vector3(nodeSize * 0.5f, nodeSize * 0.5f, nodeSize * 0.5f);

            // Check if the point is already inside the node
            if (IsPointInsideNode(point))
            {
                return point;
            }

            // Find the closest point on the surface of the node's AABB
            float closestX = Mathf.Clamp(point.x, nodePosition.x, nodePosition.x + nodeSize * .5f);
            float closestY = Mathf.Clamp(point.y, nodePosition.y, nodePosition.y + nodeSize * .5f);
            float closestZ = Mathf.Clamp(point.z, nodePosition.z, nodePosition.z + nodeSize * .5f);

            return new Vector3(closestX, closestY, closestZ);
        }

        // Check if a point is inside the node's AABB
        public bool IsPointInsideNode(Vector3 objPos)
        {
            if (objPos.x >= nodePosition.x - (nodeSize / 2) && objPos.x <= nodePosition.x + (nodeSize / 2))
            {
                if (objPos.y >= nodePosition.y - (nodeSize / 2) && objPos.y <= nodePosition.y + (nodeSize / 2))
                {
                    if (objPos.z >= nodePosition.z - (nodeSize / 2) && objPos.z <= nodePosition.z + (nodeSize / 2))
                    {
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public Dictionary<Vector3, OctreeNode> TraverseNodesWithPositions()
        {
            Queue<OctreeNode> nodeQueue = new Queue<OctreeNode>();
            nodeQueue.Enqueue(this);

            Dictionary<Vector3, OctreeNode> nodeDictionary = new Dictionary<Vector3, OctreeNode>
        {
            { this.nodePosition, this } // Add the root node to the dictionary
        };

            while (nodeQueue.Count > 0)
            {
                OctreeNode currentNode = nodeQueue.Dequeue();

                // Enqueue child nodes for further traversal
                if (currentNode.HaveChildren())
                {
                    foreach (var childNode in currentNode.nodeChildren)
                    {
                        nodeQueue.Enqueue(childNode);

                        // Add child node to the dictionary
                        if (!nodeDictionary.ContainsKey(childNode.nodePosition))
                        {
                            nodeDictionary.Add(childNode.nodePosition, childNode);
                        }
                    }
                }
            }

            return nodeDictionary;
        }

        // Method to assign depth values to nodes based on distance from the sphere's center (recursive)
        public void AssignDepthValues(Vector3 sphereCenter, float sphereRadius, int maxDepth)
        {
            GetNeighbourPositions();
            float adjustedSphere = sphereRadius;
            // Calculate the distance from the sphere's center to this node's corner


            Vector3 nodeCornerPos = nodePosition - ((nodePosition.normalized - sphereCenter.normalized) * nodeSize / 2);

            float distance = Vector3.Distance(sphereCenter, nodeCornerPos);

            // Normalize the distance based on the sphere's radius to get a value between 0 and 1
            float normalizedDepth = Mathf.Clamp01(distance / adjustedSphere);

            if (HaveChildren())
            {
                foreach (OctreeNode child in nodeChildren)
                {
                    // Recursively call the method for each child node
                    child.AssignDepthValues(sphereCenter, adjustedSphere, maxDepth);
                }
            }
            else
            {
                // If the node is a leaf node and it reaches the maxDepth, you can assign a specific value to indicate that it is a leaf.
                // You can modify this condition based on your specific requirements.
                if (nodeDepth == maxDepth)
                {
                    //depthValue = normalizedDepth;
                    //if (depthValue < 0.99f && depthValue > 0.93f)
                    //{
                    //    onSurface = true;
                    //    PushVertice(sphereCenter, sphereRadius);
                    //}

                    //if (nodePosition == new Vector3(1.95f, -0.08f, -0.08f))
                    //    Debug.Log("normalizedDepth: " + normalizedDepth);

                   if (normalizedDepth >= 1.0f) // Node is outside the sphere
                   {
                       depthValue = 0f; // Set depthValue to 0 for nodes outside the sphere
                       onSurface = false; // Not on the surface
                   }
                   else if (normalizedDepth >= 1.0f - (0.068f)) // Node is very close to the sphere's edge
                   {
                       depthValue = 1f; // Set depthValue to 1 for nodes on the surface
                       onSurface = true; // On the surface
                   }
                   else // Node is inside the sphere
                   {
                       depthValue = 1f; // Set depthValue to 1 for nodes inside the sphere
                       onSurface = false; // Not on the surface
                   }

                   //Debug.Log("nodePosition: " + nodePosition + " children?: " + HaveChildren() + " depth: " + depthValue + " neighbours: " + nodeNeighbours[0] + " n2: " + nodeNeighbours[1] + " onSurface: " + onSurface + " normalizedDepth: " + normalizedDepth);

                        if (onSurface)
                            PushVertice(sphereCenter, sphereRadius);
                }
            }
        }

        private void PushVertice(Vector3 sphereCenter, float sphereRadius)
        {
            if (onSurface)
            {
                float distance = Vector3.Distance(sphereCenter, nodePosition);

                Vector3 direction = nodePosition.normalized - sphereCenter.normalized;
                // How far can we still move from our center to the sphere edge
                float leftOverToRadius = sphereRadius - distance;

                float maxLength = nodeSize / 2; // corner Max Length Mathf.Sqrt(Mathf.Pow(size / 2, 2) + Mathf.Pow(size / 2, 2));
                float distVertice = 0;
                float jumpRange = (nodeSize / 2) / 10;

                Vector3 returnPos = nodePosition;

                for (int i = 0; i < 10; i++)
                {
                    if (distVertice < maxLength && distVertice < leftOverToRadius || distVertice > -maxLength && leftOverToRadius < 0)
                    {
                        if (leftOverToRadius > 0)
                            distVertice += jumpRange;
                        else
                            distVertice -= jumpRange;
                    }
                }
                returnPos = nodePosition + (direction * leftOverToRadius);
                voxelPoint = returnPos;
            }
        }

        public void TestTriangle()
        {
            List<Vector3> test = GetAllNeighbourVertices();
            Vector3 A = voxelPoint;
            Vector3 B = test[0];
            Vector3 C = test[1];
            
            Vector3 crossProduct = Vector3.Cross(B - A, C - A);

            Debug.Log("Voxel: " + voxelPoint);
            if (crossProduct.z > 0)
            {
                Debug.Log("Counterclockwise orientation");
            }
            else if (crossProduct.z < 0)
            {
                Debug.Log("Clockwise orientation");
            }
            else
            {
                Debug.Log("Colinear points (not a triangle)");
            }
        }

        public List<Vector3> GetAllNeighbourVertices()
        {
            List<Vector3> NeighbourVertices = new List<Vector3>();

            foreach(OctreeNode n in nodeNeighbours)
            {
                if(n.onSurface)
                {
                    NeighbourVertices.Add(n.voxelPoint);
                }
            }

            return NeighbourVertices;
        }

        // Method to get all voxelPoints from child nodes (recursive)
        public List<Vector3> GetAllVoxelPointsFromChildren()
        {
            List<Vector3> voxelPoints = new List<Vector3>();

            if (HaveChildren())
            {
                foreach (OctreeNode child in nodeChildren)
                {
                    voxelPoints.AddRange(child.GetAllVoxelPointsFromChildren());
                }
            }
            else
            {
                if (voxelPoint != null)
                {
                    voxelPoints.Add(voxelPoint);


                }
            }

            return voxelPoints;
        }            //MIGHT BE USELESS
        public List<int> GetAllTrianglesFromChildren()
        {
            List<int> triangles = new List<int>();

            if (HaveChildren())
            {
                foreach (OctreeNode child in nodeChildren)
                {
                    triangles.AddRange(child.GetAllTrianglesFromChildren());
                }
            }
            else
            {
                if (nodeTriangle[0] != 0)
                {
                    triangles.Add(nodeTriangle[0]);
                    triangles.Add(nodeTriangle[1]);
                    triangles.Add(nodeTriangle[2]);
                }
            }

            return triangles;
        }                  //MIGHT BE USELESS

    }
}
