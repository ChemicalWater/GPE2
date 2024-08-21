using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Source https://www.youtube.com/watch?v=QN39W020LqU
public class TerrainFace
{
    ShapeGenerator shapeGenerator;
    Mesh mesh;
    int resolution;
    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;
    List<int> triangles = new List<int>();
    List<Vector3> vertices = new List<Vector3>();


    public TerrainFace(ShapeGenerator shapeGenerator, Mesh mesh, int resolution, Vector3 localUp)
    {
        this.shapeGenerator = shapeGenerator;
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public void ConstructMesh()
    {
        Vector3[] vertices = new Vector3[resolution * resolution];
        //int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triIndex = 0;
        Vector2[] uv = (mesh.uv.Length == vertices.Length) ? mesh.uv : new Vector2[vertices.Length];
        
        for(int y= 0; y<resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                // How far are we in this loop
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                // Where are we
                Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;

                //vertices[i] = shapeGenerator.CalculatePointOnPlanet(pointOnUnitSphere);
                float unscaledElevation = shapeGenerator.CalculateUnscaledElevation(pointOnUnitSphere);

                vertices[i] = pointOnUnitSphere * shapeGenerator.GetScaledElevation(unscaledElevation);
                uv[i].y = unscaledElevation;

                if (x != resolution - 1 && y != resolution - 1)
                {
                    // Create all triangles
                    triangles.Add(i);
                    triangles.Add(i + resolution + 1);
                    triangles.Add(i + resolution);
                    triangles.Add(i);
                    triangles.Add(i + 1);
                    triangles.Add(i + resolution + 1);
                   //triangles[triIndex] = i;
                   //triangles[triIndex + 1] = i + resolution + 1;
                   //triangles[triIndex + 2] = i + resolution;
                   //
                   //triangles[triIndex + 3] = i;
                   //triangles[triIndex + 4] = i + 1;
                   //triangles[triIndex + 5] = i + resolution + 1;
                   //triIndex += 6;
                }
            }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.uv = uv;
    }

    void ClearData()
    {
        triangles.Clear();
        vertices.Clear();
    }

    public void UpdateUVs(ColourGenerator colourGenerator)
    {
        Vector2[] uv = mesh.uv;
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                // How far are we in this loop
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                // Where are we
                Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;

                uv[i].x = colourGenerator.BiomePercentFromPoint(pointOnUnitSphere);
            }
        }
        mesh.uv = uv;

    }
}
