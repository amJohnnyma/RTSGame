
using System.Collections.Generic;
using System.Linq;
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

    public float blend = 0.5f;

    private PerlinColor perlinColor;
    [SerializeField] private float scale = 0.01f;
    [Tooltip("Set val as exponent to base 2")]
    [SerializeField] private int perlinSize = 10;

    private float minH, maxH;
    private float minP, maxP;



    public void Init(int seed, int layers, float flatness, float heightScale)
    {
        this.seed = seed;
        this.layers = layers;
        this.flatness = flatness;
        this.heightScale = heightScale;
        perlinColor = gameObject.GetComponent<PerlinColor>();
        perlinColor.Init();
    }
    
    public void Gen(Mesh mesh, Transform terrainTransform)
    {
        int[] triangles = mesh.triangles;
        Vector3[] oldVerts = mesh.vertices;

        Vector3[] newVerts = new Vector3[triangles.Length];
        Color[] newColors = new Color[triangles.Length];
        int[] newTris = new int[triangles.Length];

        PerlinNoise noise = new PerlinNoise(seed, perlinSize, scale);

        // 1️⃣ Compute min/max height for normalized height
        Vector3[] vertices = mesh.vertices;
        float minH = float.MaxValue;
        float maxH = float.MinValue;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = terrainTransform.TransformPoint(vertices[i]);
            float h = worldPos.magnitude;
            if (h < minH) minH = h;
            if (h > maxH) maxH = h;
        }
        Debug.Log($"Min height: {minH}, Max height: {maxH}");

        // 2️⃣ Sample all Perlin values (after flatness) and store
        List<float> perlinSamples = new List<float>();
        Vector3[] centers = new Vector3[triangles.Length / 3];
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v0 = oldVerts[triangles[i]].normalized;
            Vector3 v1 = oldVerts[triangles[i + 1]].normalized;
            Vector3 v2 = oldVerts[triangles[i + 2]].normalized;
            Vector3 center = (v0 + v1 + v2) / 3f;

            float ec = noise.Val(center.x * 2f, center.y * 2f, layers, flatness);
            ec = Mathf.Clamp01(ec); // ensure 0-1 after flatness

            perlinSamples.Add(ec);
            centers[i / 3] = center; // store for later
        }

        // 3️⃣ Compute min/max of Perlin after flatness
        float minP = perlinSamples.Min();
        float maxP = perlinSamples.Max();
        Debug.Log($"Perlin Min: {minP}, Max: {maxP}");

        // Print histogram (10 bins)
        int[] histogram = new int[10];
        foreach (var val in perlinSamples)
        {
            int bin = Mathf.Clamp(Mathf.FloorToInt(val * 10f), 0, 9);
            histogram[bin]++;
        }
        Debug.Log("Perlin distribution (10 bins):");
        for (int i = 0; i < 10; i++)
        {
            float binMin = i / 10f;
            float binMax = (i + 1) / 10f;
            Debug.Log($"Bin {i} ({binMin:F1}-{binMax:F1}): {histogram[i]}");
        }

        // 4️⃣ Now assign colors and offsets
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v0 = oldVerts[triangles[i]].normalized;
            Vector3 v1 = oldVerts[triangles[i + 1]].normalized;
            Vector3 v2 = oldVerts[triangles[i + 2]].normalized;

            // Height offsets
            float e0 = noise.Val(v0.x * 2f, v0.y * 2f, layers, flatness);
            float e1 = noise.Val(v1.x * 2f, v1.y * 2f, layers, flatness);
            float e2 = noise.Val(v2.x * 2f, v2.y * 2f, layers, flatness);

            v0 *= (1f + e0 * heightScale);
            v1 *= (1f + e1 * heightScale);
            v2 *= (1f + e2 * heightScale);

            Vector3 center = centers[i / 3];

            // Normalize Perlin after flatness to 0-1
            float ec = perlinSamples[i / 3];
            float normalizedPerlin = Mathf.InverseLerp(minP, maxP, ec);

            // Normalized height
            float normalizedHeight = Mathf.InverseLerp(minH, maxH, center.magnitude);

            // Blend with height
            float finalPerlin = Mathf.Lerp(normalizedHeight, normalizedPerlin, blend);

            Color triColor = perlinColor.GetColor(finalPerlin);

            // Assign duplicated vertices
            newVerts[i] = v0;
            newVerts[i + 1] = v1;
            newVerts[i + 2] = v2;

            newColors[i] = triColor;
            newColors[i + 1] = triColor;
            newColors[i + 2] = triColor;

            // Triangle indices (winding)
            if (Vector3.Dot(Vector3.Cross(v1 - v0, v2 - v0), v0) < 0f)
            {
                newTris[i] = i;
                newTris[i + 1] = i + 2;
                newTris[i + 2] = i + 1;
            }
            else
            {
                newTris[i] = i;
                newTris[i + 1] = i + 1;
                newTris[i + 2] = i + 2;
            }
        }

        // Apply to mesh
        mesh.vertices = newVerts;
        mesh.triangles = newTris;
        mesh.colors = newColors;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }


/*
            public void Gen(Mesh mesh, Transform terrainTransform)
            {
                int[] triangles = mesh.triangles;
                Vector3[] oldVerts = mesh.vertices;

                Vector3[] newVerts = new Vector3[triangles.Length];
                Color[] newColors = new Color[triangles.Length];
                int[] newTris = new int[triangles.Length];

                PerlinNoise noise = new PerlinNoise(seed, perlinSize, scale);


                Vector3[] vertices = mesh.vertices;

                maxH = float.MinValue;
                minH = float.MaxValue;

                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 worldPos = terrainTransform.TransformPoint(vertices[i]);
                    float h = worldPos.magnitude;
                    if (h < minH) minH = h;
                    if (h > maxH) maxH = h;

                }
                Debug.Log($"Min height (h): {minH}, Max height (h): {maxH}");

                // 1. Sample all triangle centers
                List<float> perlinSamples = new List<float>();
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    Vector3 v0 = oldVerts[triangles[i]].normalized;
                    Vector3 v1 = oldVerts[triangles[i + 1]].normalized;
                    Vector3 v2 = oldVerts[triangles[i + 2]].normalized;
                    Vector3 center = (v0 + v1 + v2) / 3f;

                    float ec = noise.Val(center.x * 2f, center.y * 2f, layers, flatness);
                    perlinSamples.Add(ec);
                }

                // 2. Compute min/max
                float minSample = perlinSamples.Min();
                float maxSample = perlinSamples.Max();
                Debug.Log($"Perlin Min: {minSample}, Max: {maxSample}");


                for (int i = 0; i < triangles.Length; i += 3)
                {
                    Vector3 v0 = oldVerts[triangles[i]].normalized;
                    Vector3 v1 = oldVerts[triangles[i + 1]].normalized;
                    Vector3 v2 = oldVerts[triangles[i + 2]].normalized;
                    Vector3 center = (v0 + v1 + v2) / 3f;

                    float ec = noise.Val(center.x * 2f, center.y * 2f, layers, flatness);

                    // Now normalized 0-1
                    float normalizedPerlin = Mathf.InverseLerp(minSample, maxSample, ec);

                    // Blend with height if needed
                    float normalizedHeight = Mathf.InverseLerp(minH, maxH, center.magnitude);
                    float finalPerlin = Mathf.Lerp(normalizedHeight, normalizedPerlin, blend);

                    // Use finalPerlin for color
                    Color triColor = perlinColor.GetColor(finalPerlin);

                    // Assign duplicated vertices
                    newVerts[i] = v0;
                    newVerts[i + 1] = v1;
                    newVerts[i + 2] = v2;

                    // Same color for all three
                    newColors[i] = triColor;
                    newColors[i + 1] = triColor;
                    newColors[i + 2] = triColor;

                    // New triangle indices
                    // enforce winding (swap if needed)
                    if (Vector3.Dot(Vector3.Cross(v1 - v0, v2 - v0), v0) < 0f)
                    {
                        // flip winding
                        newTris[i] = i;
                        newTris[i + 1] = i + 2;
                        newTris[i + 2] = i + 1;
                    }
                    else
                    {
                        newTris[i] = i;
                        newTris[i + 1] = i + 1;
                        newTris[i + 2] = i + 2;
                    }
                }

                // Apply back
                mesh.vertices = newVerts;
                mesh.triangles = newTris;
                mesh.colors = newColors;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                Debug.Log("SCALED\n\n");

            }
        */


}
