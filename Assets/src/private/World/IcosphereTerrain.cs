using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(MeshFilter))]
public class IcosphereTerrain : MonoBehaviour
{
    private int seed = 1337;
    private int layers = 5;
    private float flatness = 1.5f;
    private float heightScale = 1f;

    private PerlinColor perlinColor;



    public void Init(int seed, int layers, float flatness, float heightScale)
    {
        this.seed = seed;
        this.layers = layers;
        this.flatness = flatness;
        this.heightScale = heightScale;
        perlinColor = gameObject.GetComponent<PerlinColor>();
        perlinColor.Init();
    }

    public void Gen(Mesh mesh)
    {
        int[] triangles = mesh.triangles;
        Vector3[] oldVerts = mesh.vertices;

        Vector3[] newVerts = new Vector3[triangles.Length];
        Color[] newColors = new Color[triangles.Length];
        int[] newTris = new int[triangles.Length];

        PerlinNoise noise = new PerlinNoise(seed);

        for (int i = 0; i < triangles.Length; i += 3)
        {
            // Get triangle vertices
            Vector3 v0 = oldVerts[triangles[i]].normalized;
            Vector3 v1 = oldVerts[triangles[i+1]].normalized;
            Vector3 v2 = oldVerts[triangles[i+2]].normalized;

            // Offset vertices by noise-based height
            float e0 = noise.Val(v0.x * 2f, v0.y * 2f, layers, flatness);
            float e1 = noise.Val(v1.x * 2f, v1.y * 2f, layers, flatness);
            float e2 = noise.Val(v2.x * 2f, v2.y * 2f, layers, flatness);

            v0 *= (1f + e0 * heightScale);
            v1 *= (1f + e1 * heightScale);
            v2 *= (1f + e2 * heightScale);

            // Use triangle center for color
            Vector3 center = (v0 + v1 + v2) / 3f;
            float ec = noise.Val(center.x * 2f, center.y * 2f, layers, flatness);
            Color triColor = perlinColor.GetColor(ec * heightScale * flatness * layers);

            // Assign duplicated vertices
            newVerts[i]   = v0;
            newVerts[i+1] = v1;
            newVerts[i+2] = v2;

            // Same color for all three
            newColors[i]   = triColor;
            newColors[i+1] = triColor;
            newColors[i+2] = triColor;

            // New triangle indices
            newTris[i]   = i;
            newTris[i+1] = i+1;
            newTris[i+2] = i+2;
        }

        // Apply back
        mesh.vertices = newVerts;
        mesh.triangles = newTris;
        mesh.colors = newColors;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

}
