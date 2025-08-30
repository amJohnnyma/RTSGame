using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEditor.Il2Cpp;
using UnityEngine;

public class World : MonoBehaviour
{
    [Header("Base world")]
    [SerializeField] public float radius = 1f;
    [SerializeField][Range(0, 6)] public int lodSubdivisions = 2;
    [SerializeField][Range(0, 6)] public int initialSubdivisions = 2;
    [SerializeField] public int splitDistance = 100;
    [SerializeField] public int mergeDistance = 200;
    [SerializeField] public int lodJumpDistance = 50;
    [SerializeField] public int numVertices;

    [Header("Perlin terrain")]
    [SerializeField] private int seed;
    [SerializeField] private int layers;
    [SerializeField] private float flatness;
    [SerializeField] private float height;
    [SerializeField] private Camera cam;


    private GameObject sphere;
    private GameObject sphereChunks;
    private EntitySpawner spawner;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    private IcosphereGenerator icoSphereGen = new IcosphereGenerator();
    private IcosphereTerrain terrain;
    private List<Chunk> chunks = new List<Chunk>();


    private void OnValidate()
    {
        //CreateIcosphere();

    }

    private void Start()
    {
        CreateIcosphere();
    }


    private void FixedUpdate()
    {
        foreach (Chunk c in chunks)
        {
            if (c == null)
            {
                Debug.Log("Null chunk");
                continue;
            }
            if (c.depth >= lodSubdivisions)
            {
                continue;
            }
            float dist = Vector3.Distance(cam.transform.position, c.Center());
          //  Debug.Log("Distance : " + dist);

            if (dist < splitDistance && c.isLeaf)
            {
                Debug.Log("Split");
                
                int targetSub = Mathf.Clamp(
                    Mathf.CeilToInt(dist / lodJumpDistance),
                    1, lodSubdivisions
                );
                Debug.Log("Splitting: " + targetSub);

                icoSphereGen.RecSubdivide(c, radius, targetSub, lodSubdivisions);
                c.Activate(true);
            }
            else if (dist < mergeDistance && !c.isLeaf && dist > splitDistance)
            {
                Debug.Log("Merge");

                int[] del = icoSphereGen.Collapse(c);
                foreach (int i in del)
                {
                    Destroy(c.children[i].go);
                }


                c.children = null;
                c.isSubdivided = false;
                c.isLeaf = true;

                c.go.GetComponent<MeshRenderer>().enabled = true;
                c.Activate(true);

            }

        }
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
                //   meshRenderer.sharedMaterial = new Material(Shader.Find("src/Materials/WorldMat"));
                meshCollider = sphere.AddComponent<MeshCollider>();
                terrain = sphere.AddComponent<IcosphereTerrain>();
                spawner = sphere.AddComponent<EntitySpawner>();
            }

        }
        Transform chunkS = sphere.transform.Find("Chunks");

        if (chunkS != null)
        {
            Destroy(chunkS.gameObject);

        }
        sphereChunks = new GameObject("Chunks");
        sphereChunks.transform.SetParent(sphere.transform, false);
        chunks = icoSphereGen.Create(radius, initialSubdivisions, sphereChunks);

        // Generate mesh and assign
        if (meshFilter != null)
        {
            //         meshFilter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            //meshFilter.sharedMesh = icoSphereGen.Create(radius, initialSubdivisions);

            //          terrain.Init(seed, layers, flatness, height);
            //            terrain.Gen(meshFilter.sharedMesh);

            //          meshCollider.sharedMesh = null;
            //            meshCollider.sharedMesh = meshFilter.sharedMesh;

            //  spawner.SpawnOnTerrain(meshFilter.sharedMesh);

        }


        numVertices = (int)(10f * Mathf.Pow(4, initialSubdivisions) + 2f);

    }

    public List<float> getWorldVars()
    {
        return new List<float> { layers, flatness, height };
    }
    Vector3 midpoint(Vector3 a, Vector3 b)
    {
        return (a + b).normalized;
    }

}
