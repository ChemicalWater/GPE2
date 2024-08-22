using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace Test.Octree
{
    public class OctreeNode2
    {
        public float scalarValue { get; set; }

        public int identify;

        public Vector3 nodePosition;
        public float nodeSize;
        public float depthValue;
        public OctreeNode2 parent;
        public Vector3 voxelPoint;
        public List<OctreeNode2> nodeNeighbour = new List<OctreeNode2>();
        public OctreeNode2[] nodeChildren = new OctreeNode2[8];
        private Vector3[] nodeChildrenPos = new Vector3[8];
        public bool leafNode;

        private Vector3 octreeCenter;
        private float sphereRadius;

        public List<Vector3> nodeTriangles = new List<Vector3>();

        public List<Vector3> nodeVertices = new List<Vector3>();

        public float[] cornerValues = new float[8];
        
        public int nodeDepth { get; private set; }
        public int standardDepth { get; private set; }

        public OctreeNode2 (Vector3 nodePos, float size, int myDepth, int standardDepth, OctreeNode2 nodeParent)
        {
            this.nodePosition = nodePos;
            this.nodeSize = size;
            this.nodeDepth = myDepth;
            this.standardDepth = standardDepth;
            this.parent = nodeParent;

            scalarValue = 0;
            Subdivide();
        }

        public Vector3[] GetCorners()
        {
            float halfSize = nodeSize / 2;
            return new Vector3[]
            {
        nodePosition + new Vector3(-halfSize, -halfSize, -halfSize),  // 0
        nodePosition + new Vector3(halfSize, -halfSize, -halfSize),   // 1
        nodePosition + new Vector3(halfSize, halfSize, -halfSize),    // 2
        nodePosition + new Vector3(-halfSize, halfSize, -halfSize),   // 3
        nodePosition + new Vector3(-halfSize, -halfSize, halfSize),   // 4
        nodePosition + new Vector3(halfSize, -halfSize, halfSize),    // 5
        nodePosition + new Vector3(halfSize, halfSize, halfSize),     // 6
        nodePosition + new Vector3(-halfSize, halfSize, halfSize)     // 7
            };
        }

        public float[] CornerValues()
        {
            return cornerValues;
        }

        public float EvaluateScalarField(Vector3 position, float radius, Vector3 center)
        {
            if (octreeCenter == new Vector3())
                octreeCenter = center;
            if (sphereRadius == 0)
                sphereRadius = radius;

            // Distance-based density (e.g., spherical)
            float distanceFromCenter = Vector3.Distance(position, center);
            float baseValue = Mathf.Clamp01(1f - (distanceFromCenter / radius));

            return Mathf.Clamp01(baseValue);
        }

        public Vector3[] GetNeighbourPositions()
        {
            Vector3 mySize = new Vector3(nodeSize, nodeSize, nodeSize);
            return new Vector3[]
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
        }

        private void GetChildPositions()
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

        public bool HaveChildren()
        {
            if (nodeChildren[0] == null)
            {
                leafNode = true;
                    return false;
            }
            else
            {
                leafNode = false;
                return true;
            } 
        }

        public OctreeNode2 FindNodeContainingPoint(OctreeNode2 node, Vector3 vertex)
        {
            // Check if the point is inside the current node and if the node is not at maximum depth
            if (node.IsPointInsideNode(vertex) && node.HaveChildren())
            {
                    // Traverse through the child nodes to find the correct node containing the vertex.
                    foreach (OctreeNode2 child in node.nodeChildren)
                    {
                        if (child.IsPointInsideNode(vertex))
                        {
                            // Recursively call the method on the child node that contains the vertex.
                            return FindNodeContainingPoint(child, vertex); // Return the result from the recursive call
                        }
                    }
            }

            // No children? return currentnode as node containing vertex
            return node;
        }

        // Function to place a vertex inside the octree

        //Split this node into children
        public void Subdivide()
        {
            GetChildPositions();
            if (nodeDepth < standardDepth)
            {
                for(int i = 0; i < nodeChildrenPos.Length; i++)
                {
                    nodeChildren[i] = (new OctreeNode2(nodeChildrenPos[i], (nodeSize * .5f), (nodeDepth+1),standardDepth, this));
                    nodeChildren[i].Subdivide();
                }
            }
            HaveChildren();
        }

       public void SubdivideIfNeeded(int maxDepth, float[] parentValues, Vector3[] parentCorners)
       {
            float[] parentV = parentValues;
            Vector3[] parentC = parentCorners;
           if(nodeDepth < maxDepth)
           {
                leafNode = false;
               for (int i = 0; i < nodeChildrenPos.Length; i++)
               {
                   nodeChildren[i] = (new OctreeNode2(nodeChildrenPos[i], (nodeSize * .5f), (nodeDepth + 1), standardDepth, this));
                   nodeChildren[i].leafNode = true;
                   nodeChildren[i].SubdivideIfNeeded(maxDepth, parentValues, parentCorners);
                }
           }
           if(leafNode)
            for (int i = 0; i < 8; i++)
                {
                    cornerValues[i] = EvaluateScalarField(GetCorners()[i], 2f, Vector3.zero);
                }
            //HaveChildren();
        }

        // combine this node
        public void Undivide()
        {
            // Only proceed if this node has children
            if (HaveChildren())
            {
                // Recursively undivide all children first
                foreach (OctreeNode2 child in nodeChildren)
                {
                    child.Undivide();
                }

                // Clear the child nodes
                nodeChildren = new OctreeNode2[8];

                // If you have any data to consolidate from children to the parent node, do it here.
                // For example, update parent node values based on child nodes.
                // This part depends on your specific use case. Here is a generic example:
                // cornerValues could be updated or re-evaluated here based on child node data.

                leafNode = true; // This node becomes a leaf node after undividing
            }
        }

        private void PushVertice(Vector3 sphereCenter, float sphereRadius)
        {
            if (leafNode)
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

        // Check if a point is inside the node
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

        // Method to traverse the octree and collect all nodes
        public Dictionary<Vector3, OctreeNode2> TraverseOctree()
        {
            Dictionary<Vector3, OctreeNode2> allNodes = new Dictionary<Vector3, OctreeNode2>();
            TraverseNode(this, allNodes);
            return allNodes;
        }

        // Recursive method to traverse each node and its children
        private static void TraverseNode(OctreeNode2 node, Dictionary<Vector3, OctreeNode2> nodeList)
        {
            if (node == null)
                return;

            // Add the current node to the list
            if(!nodeList.ContainsValue(node))
                nodeList.Add(node.nodePosition, node);

            // Traverse all children
            foreach (OctreeNode2 child in node.nodeChildren)
            {
                TraverseNode(child, nodeList);
            }
        }
    }
}
