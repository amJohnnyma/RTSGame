using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEditor.Il2Cpp;
using UnityEngine;
using UnityEngine.Rendering;

public class World : MonoBehaviour
{
    [Header("Base world")]
    [SerializeField] public float radius = 1f;
    [SerializeField][Range(0, 6)] public int subdivisions = 2;
    [SerializeField] public int numVertices;

    [Header("Perlin terrain")]
    [SerializeField] private int seed;
    [SerializeField] private int layers;
    [SerializeField] private float flatness;
    [SerializeField] private float height;


    private GameObject sphere;
    private EntitySpawner spawner;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    private IcosphereGenerator icoSphereGen = new IcosphereGenerator();
    private IcosphereTerrain terrain;

    private Dictionary<Vector3, GameObject> placedEntities = new Dictionary<Vector3, GameObject>();


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
                meshCollider = sphere.GetComponent<MeshCollider>();
                terrain = sphere.GetComponent<IcosphereTerrain>();
                spawner = sphere.GetComponent<EntitySpawner>();
            }
            else
            {
                sphere = new GameObject("Icosphere");
                sphere.transform.SetParent(transform, false);

                meshFilter = sphere.AddComponent<MeshFilter>();
                meshRenderer = sphere.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = new Material(Shader.Find("WorldMat"));
                meshCollider = sphere.AddComponent<MeshCollider>();
                terrain = sphere.AddComponent<IcosphereTerrain>();
                spawner = sphere.AddComponent<EntitySpawner>();
            }
        }

        // Generate mesh and assign
        if (meshFilter != null)
        {

            meshFilter.sharedMesh = icoSphereGen.Create(radius, subdivisions);
            terrain.Init(seed, layers, flatness, height);
            terrain.Gen(meshFilter.sharedMesh);

            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = meshFilter.sharedMesh;

            spawner.SpawnOnTerrain(meshFilter.sharedMesh);

        }


        numVertices = (int)(10f * Mathf.Pow(4, subdivisions) + 2f);

    }

    public List<float> getWorldVars()
    {
        return new List<float> { layers, flatness, height };
    }

    public void AddPlacedEntity(Vector3 key, GameObject go)
    {
        placedEntities[key] = go;
        Debug.Log("Added at " + key.ToString());
    }
    public void DestroyPlacedEntity(Vector3 key)
    {
        Destroy(placedEntities[key]);
        placedEntities.Remove(key);
        Debug.Log("Destroy at " + key.ToString());

    }
    public GameObject GetPlacedEntity(Vector3 key)
    {
        if (isPlacedEntityPresent(key))
        {
            return placedEntities[key];
        }
        return null;
    }

    public bool isPlacedEntityPresent(Vector3 key)
    {
        return placedEntities.ContainsKey(key);
    }
}
