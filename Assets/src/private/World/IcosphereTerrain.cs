using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class IcosphereTerrain : MonoBehaviour
{
    private int seed = 1337;
    private int layers = 5;
    private float flatness = 1.5f;
    private float heightScale = 1f;

    public void Init(int seed, int layers, float flatness, float heightScale)
    {
        this.seed = seed;
        this.layers = layers;
        this.flatness = flatness;
        this.heightScale = heightScale;
        
    }

    public void Gen()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        Vector3[] vertices = mesh.vertices;

        PerlinNoise noise = new PerlinNoise(seed);

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i].normalized; // stay on sphere
            float e = noise.Val(v.x * 2f, v.y * 2f, layers, flatness);
            vertices[i] = v * (1f + e * heightScale);
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
