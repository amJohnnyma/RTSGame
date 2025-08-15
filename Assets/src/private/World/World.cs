using System.Collections;
using System.Collections.Generic;
using UnityEditor.Il2Cpp;
using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField] public float radius = 1f;
    [SerializeField][Range(0,6)] public int subdivisions = 2;
    [SerializeField] public int numVertices;


    private GameObject sphere;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;


    private void OnValidate()
    {
        CreateIcosphere();
    }

    private void Start()
    {
        CreateIcosphere();
    }


    private void CreateIcosphere()
    {
        // If sphere already exists, reuse it
        if (sphere == null)
        {
            // Try to find an existing one in children
            Transform existing = transform.Find("Icosphere");
            if (existing != null)
            {
                sphere = existing.gameObject;
                meshFilter = sphere.GetComponent<MeshFilter>();
                meshRenderer = sphere.GetComponent<MeshRenderer>();
            }
            else
            {
                sphere = new GameObject("Icosphere");
                sphere.transform.SetParent(transform, false);

                meshFilter = sphere.AddComponent<MeshFilter>();
                meshRenderer = sphere.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
            }
        }

        // Generate mesh and assign
        if (meshFilter != null)
            meshFilter.sharedMesh = IcosphereGenerator.Create(radius, subdivisions);

        numVertices = (int)(10f * Mathf.Pow(4, subdivisions) + 2f);
    
    }
}
