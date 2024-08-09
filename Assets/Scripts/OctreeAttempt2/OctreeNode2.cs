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

        public Vector3 nodePosition;
        public float nodeSize;
        public float depthValue;
        public OctreeNode2 parent;
        public Vector3 voxelPoint;
        public OctreeNode2[] nodeChildren = new OctreeNode2[8];
        private Vector3[] nodeChildrenPos = new Vector3[8];
        public bool leafNode;
        public bool bossNode;

        public Vector3[] testVertices = new Vector3[12];

        public List<Vector3> nodeTriangles = new List<Vector3>();

        public float[] cornerValues = new float[8];
        
        public int nodeDepth { get; private set; }
        public int maxDepth { get; private set; }

        public OctreeNode2 (Vector3 nodePos, float size, int myDepth, int maxDepth, OctreeNode2 nodeParent)
        {
            this.nodePosition = nodePos;
            this.nodeSize = size;
            this.nodeDepth = myDepth;
            this.maxDepth = maxDepth;
            this.parent = nodeParent;

            scalarValue = 0;
            //FindVertices();
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

        public void AssignScalarValues(OctreeNode2 node, float radius, Vector3 center)
        {
            if (leafNode)
            {
                float distanceFromCenter = Vector3.Distance(nodePosition, center) - radius;

                scalarValue = (radius - distanceFromCenter) / radius;

                for (int i = 0; i < 8; i++)
                {
                    Vector3 cornerPos = GetCorners()[i];
                    float cornerDistanceFromCenter = Vector3.Distance(cornerPos, center) - radius;
                    cornerValues[i] = (radius - cornerDistanceFromCenter) / radius;
                    //if (cornerValues[i] < 0)
                    //    cornerValues[i] = 0;
                    //else
                    //    cornerValues[i] = 1;
                }

                //PushVertice(center, radius);
                return;
            }

            foreach (var child in nodeChildren)
            {
                child.AssignScalarValues(child, radius, center);
            }
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

        private void FindVertices()
        {
            float halfSize = nodeSize / 2f;
            testVertices[0] = (new Vector3(nodePosition.x - halfSize, nodePosition.y - halfSize, nodePosition.z));
            testVertices[1] = (new Vector3(nodePosition.x - halfSize, nodePosition.y + halfSize, nodePosition.z));
            testVertices[2] = (new Vector3(nodePosition.x + halfSize, nodePosition.y - halfSize, nodePosition.z));
            testVertices[3] = (new Vector3(nodePosition.x + halfSize, nodePosition.y + halfSize, nodePosition.z));
            testVertices[4] = (new Vector3(nodePosition.x, nodePosition.y - halfSize, nodePosition.z - halfSize));
            testVertices[5] = (new Vector3(nodePosition.x, nodePosition.y - halfSize, nodePosition.z + halfSize));
            testVertices[6] = (new Vector3(nodePosition.x, nodePosition.y + halfSize, nodePosition.z - halfSize));
            testVertices[7] = (new Vector3(nodePosition.x, nodePosition.y + halfSize, nodePosition.z + halfSize));
            testVertices[8] = (new Vector3(nodePosition.x + halfSize, nodePosition.y, nodePosition.z - halfSize));
            testVertices[9] = (new Vector3(nodePosition.x + halfSize, nodePosition.y, nodePosition.z + halfSize));
            testVertices[10] = (new Vector3(nodePosition.x - halfSize, nodePosition.y, nodePosition.z - halfSize));
            testVertices[11] = (new Vector3(nodePosition.x - halfSize, nodePosition.y, nodePosition.z + halfSize));
        }

        public OctreeNode2 FindNodeContainingPoint(OctreeNode2 node, Vector3 vertex)
        {
            // Check if the point is inside the current node and if the node is not at maximum depth
            if (node.IsPointInsideNode(vertex) && node.nodeDepth != maxDepth)
            {
                if (node.HaveChildren())
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
                else
                {
                    // Subdivide the node and then recheck the point in the subdivided nodes
                    node.Subdivide();
                    return FindNodeContainingPoint(node, vertex); // Return the result after subdivision
                }
            }

            // If the point is not inside the current node or it's at max depth, return the current node
            return node;
        }


        // Function to place a vertex inside the octree
        public void PlaceVertexInNode(OctreeNode2 node, Vector3 vertex)
        {
            if (node.IsPointInsideNode(vertex) && node.nodeDepth != maxDepth)
            {
                if(node.HaveChildren())
                {
                    // Traverse through the child nodes to find the correct node containing the vertex.
                    foreach (OctreeNode2 child in node.nodeChildren)
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
                    node.Subdivide();
                }
            } else
            {
                node.voxelPoint = ClosestPointInsideNode(vertex);
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
                    nodeChildren[i] = (new OctreeNode2(nodeChildrenPos[i], (nodeSize * .5f), (nodeDepth+1) ,maxDepth, this));
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
                foreach(OctreeNode2 child in nodeChildren)
                {
                    child.Undivide();
                }
            }
            nodeChildren = null;
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

        // Method to traverse the octree and collect all nodes
        public List<OctreeNode2> TraverseOctree()
        {
            List<OctreeNode2> allNodes = new List<OctreeNode2>();
            TraverseNode(this, allNodes);
            return allNodes;
        }

        // Recursive method to traverse each node and its children
        private static void TraverseNode(OctreeNode2 node, List<OctreeNode2> nodeList)
        {
            if (node == null)
                return;

            // Add the current node to the list
            nodeList.Add(node);

            // Traverse all children
            foreach (OctreeNode2 child in node.nodeChildren)
            {
                TraverseNode(child, nodeList);
            }
        }
    }
}
