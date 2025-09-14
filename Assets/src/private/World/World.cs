using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
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

    public Dictionary<Vector3, GameObject> placedEntities = new();
    private Dictionary<Vector3, GameObject> foundHarvestables = new();
    private Dictionary<Vector3, GameObject> buildings = new();


    private void OnValidate()
    {
        CreateIcosphere();

    }

    private void Start()
    {
        CreateIcosphere();

        //also adds preplaced buildings
        spawner.SpawnOnTerrain(meshFilter.sharedMesh, this.gameObject.transform, this);
        //Track all spawned harvestables

        // add buildings
        AddBuildingsFromPlaced();

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
            terrain.Gen(meshFilter.sharedMesh, this.gameObject.transform);

            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = meshFilter.sharedMesh;


        }


        numVertices = (int)(10f * Mathf.Pow(4, subdivisions) + 2f);

    }

    public List<float> getWorldVars()
    {
        return new List<float> { layers, flatness, height };
    }

    public void AddPlacedEntity(Vector3 key, GameObject go)
    {
        try
        {

            placedEntities.Add(key,go);

        }
        catch(Exception e)
        {
            Debug.Log(e);

        }


        if (go.GetComponent<EntityStats>().type == Type.BUILDING)
        {
            try
            {
                buildings.Add(key,go);


            }
            catch(Exception e)
            {
                Debug.Log(e);

            }
        }
        //   Debug.Log("Added at " + key.ToString());
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
    public GameObject GetRandomPlacedEntity()
    {

        // Convert dictionary keys/values to a list
        var values = placedEntities.Values.ToList();

        // Pick a random index
        int randomIndex = UnityEngine.Random.Range(0, values.Count);

        return values[randomIndex];
    }


    public GameObject GetRandomPlacedHarvestable()
    {
        GameObject selected = null;
        int count = 0;

        foreach (var kvp in placedEntities)
        {
            if (kvp.Value == null) continue;
            var stats = kvp.Value.GetComponent<EntityStats>();
            if (stats != null && stats.type == Type.HARVESTABLE)
            {
                count++;
                // Reservoir sampling: each item has 1/count chance to be picked
                if (UnityEngine.Random.Range(0, count) == 0)
                    selected = kvp.Value;
            }
        }

        return selected; // null if no harvestables exist
    }

    public GameObject GetRandomUnfoundPlacedHarvestable()
    {
        GameObject selected = null;
        int count = 0;

        foreach (var kvp in placedEntities)
        {
            var stats = kvp.Value.GetComponent<EntityStats>();
            if (stats != null && stats.type == Type.HARVESTABLE)
            {
                // Only consider harvestables not yet found
                if (GetFoundHarvestable(kvp.Key) == null)
                {
                    count++;
                    if (UnityEngine.Random.Range(0, count) == 0) // reservoir sampling
                        selected = kvp.Value;
                }
            }
        }

        return selected; // null if none exist
    }

    public GameObject GetRandomFoundHarvestable()
    {

        if (foundHarvestables.Count <= 0) return null;
        var vals = foundHarvestables.Values.ToList();

        int randomIndex = UnityEngine.Random.Range(0, vals.Count);

        return vals[randomIndex];
    }





    public bool isPlacedEntityPresent(Vector3 key)
    {
        return placedEntities.ContainsKey(key);
    }

    public void AddFoundHarvestable(Vector3 key, GameObject go)
    {

        try {
        foundHarvestables.Add(key, go);

        } catch (Exception e) { Debug.Log(e); }

        Debug.Log("Harvestable added at " + key.ToString());

        

    }

    public void DestroyFoundHarvestable(Vector3 key)
    {
        if (foundHarvestables.ContainsKey(key))
        {
            GameObject go = foundHarvestables[key];
            foundHarvestables.Remove(key);
            placedEntities.Remove(key);
            Destroy(go);
            Debug.Log("Harvestable destroyed at " + key.ToString());
        }
    }

    public GameObject GetFoundHarvestable(Vector3 key)
    {
        if (IsFoundHarvestablePresent(key))
        {
            return foundHarvestables[key];
        }
        return null;
    }

    public bool IsFoundHarvestablePresent(Vector3 key)
    {
        return foundHarvestables.ContainsKey(key);
    }



    // snapshots
    private List<Vector3> unfoundHarvestablePositions = new List<Vector3>();

    public void RefreshHarvestableCache()
    {
        unfoundHarvestablePositions.Clear();

        foreach (var kvp in placedEntities)
        {
            if (kvp.Value == null) return;
            var stats = kvp.Value.GetComponent<EntityStats>();
            if (stats != null && stats.type == Type.HARVESTABLE && !IsFoundHarvestablePresent(kvp.Key))
            {
                unfoundHarvestablePositions.Add(kvp.Key);
            }
        }
    }

    public IReadOnlyList<Vector3> GetUnfoundHarvestableSnapshot()
    {
        return unfoundHarvestablePositions;
    }

    private void AddBuildingsFromPlaced()
    {
        foreach (var (k, v) in placedEntities)
        {
            if (v.GetComponent<EntityStats>().type == Type.BUILDING)
            {
                try
                {
                    buildings.Add(k, v);

            
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }
    }

    
    

}
