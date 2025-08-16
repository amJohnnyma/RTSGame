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
        Vector3[] vertices = mesh.vertices;
        Color[] colors = new Color[mesh.vertices.Length];

        PerlinNoise noise = new PerlinNoise(seed);

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i].normalized; // stay on sphere
            float e = noise.Val(v.x * 2f, v.y * 2f, layers, flatness);
            vertices[i] = v * (1f + e * heightScale);
            colors[i] = perlinColor.GetColor(e * heightScale * flatness * layers);

        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.colors = colors;
        mesh.RecalculateBounds();

    }
}
