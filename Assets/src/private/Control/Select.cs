using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Select : MonoBehaviour
{

    public List<GameObject> presets;

    [SerializeField] private World world;
    [SerializeField] private UIDocument entityPopup;
    private VisualElement popupRoot;


    [SerializeField] private bool canBuild;
    [SerializeField] private bool canDestroy;
    [SerializeField] private bool isSelecting;
    private int buildOption = -1;


    [SerializeField] private MeshCollider refMesh;
    [SerializeField] private GridHighlighter gridHighlighter;

    // Start is called before the first frame update
    void Start()
    {
        if (gridHighlighter != null)
        {
            gridHighlighter.enabled = false;
        }

        popupRoot = entityPopup.rootVisualElement;
        popupRoot.style.display = DisplayStyle.None;


    }

    // Update is called once per frame
    void Update()
    {

        CheckInput();
        UpdateHighlightGrid();
    }

    private void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Key down");
            RayHitMesh();
        }
        if (Input.GetMouseButton(0))
        {
            Debug.Log("Mouse down");
            if (canBuild)
            {
                Debug.Log("Build");
                SpawnEntity();
            }
            else if (canDestroy)
            {
                Debug.Log("Destroy");
                DestroyEntity();
            }
            else if (isSelecting)
            {
                Debug.Log("Select");
                SelectEntity();
            }
            else
            {
                Debug.Log("None");
            }
        }
        if (canBuild || canDestroy || isSelecting)
        {
            gridHighlighter.enabled = true;
        }
        else
        {
            gridHighlighter.enabled = false;
        }
    }

    private void UpdateHighlightGrid()
    {
        switch (buildOption)
        {
            case 1:
                gridHighlighter.setPatchRadius(1);
                break;
            case 2:
                gridHighlighter.setPatchRadius(4);
                break;
            case 3:
                gridHighlighter.setPatchRadius(16);
                break;
            case -1:
                gridHighlighter.setPatchRadius(1);
                break;
        }
    }

    private bool MouseDown(int type)
    {
        return Input.GetMouseButtonDown(type);
    }

    private Ray CameraToPointRay()
    {
        return Camera.main.ScreenPointToRay(Input.mousePosition);
    }


    private void HoverMouseSelected()
    {


    }


    private void RayHitMesh()
    {
        //Debug.Log("Ray");
        Ray ray = CameraToPointRay();
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            //   Debug.Log("Ray hit");
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider != null)
            {
                //     Debug.Log("Collider found");
                Mesh mesh = meshCollider.sharedMesh;
                int triIndex = hit.triangleIndex;

                int i0 = mesh.triangles[triIndex * 3 + 0];
                int i1 = mesh.triangles[triIndex * 3 + 1];
                int i2 = mesh.triangles[triIndex * 3 + 2];

                Vector3 p0 = mesh.vertices[i0];
                Vector3 p1 = mesh.vertices[i1];
                Vector3 p2 = mesh.vertices[i2];

                //  Debug.Log($"Hit triangle {triIndex}: verts {i0},{i1},{i2}");

                Color[] colors = mesh.colors;
                colors[i0] = Color.red;
                colors[i1] = Color.red;
                colors[i2] = Color.red;

                mesh.colors = colors;

                meshCollider.sharedMesh = null;
                meshCollider.sharedMesh = mesh;

            }
        }
    }
    private (MeshCollider, RaycastHit) HasHitMesh()
    {
        //Debug.Log("Ray");
        Ray ray = CameraToPointRay();
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            //   Debug.Log("Ray hit");
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider != null)
            {

                return (meshCollider, hit);

            }

            return (null, hit);
        }

        return (null, default);

    }

    private void SpawnEntity()
    {
        if (!canBuild || buildOption <= 0)
        {
            return;
        }


        (MeshCollider meshCollider, RaycastHit hit) = HasHitMesh();
        if (meshCollider != null)
        {
            //     Debug.Log("Collider found");
            Mesh mesh = meshCollider.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;

            int triIndex = hit.triangleIndex;

            int i0 = mesh.triangles[triIndex * 3 + 0];
            int i1 = mesh.triangles[triIndex * 3 + 1];
            int i2 = mesh.triangles[triIndex * 3 + 2];

            Vector3 v0 = vertices[i0];
            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];

            v0 = transform.TransformPoint(v0);
            v1 = transform.TransformPoint(v1);
            v2 = transform.TransformPoint(v2);

            Vector3 pos = (v0 + v1 + v2) / 3f;
            if (world.isPlacedEntityPresent(pos)) return;

            // Interpolate normal across the triangle instead of picking one vertex
            Vector3 baryCenter = hit.barycentricCoordinate;
            Vector3 normal =
                -normals[i0] * baryCenter.x +
                -normals[i1] * baryCenter.y +
                -normals[i2] * baryCenter.z;
            Quaternion rot = Quaternion.FromToRotation(normal, normal.normalized);

            Debug.Log("Instantiate");
            // Parent new instances under container`
            GameObject go = Instantiate(presets[buildOption - 1], pos, rot, gameObject.transform) as GameObject;
            go.tag = "Entity";
            world.AddPlacedEntity(pos, go);

        }

    }

    private void DestroyEntity()
    {
        if (!canDestroy)
        {
            return;
        }


        (MeshCollider meshCollider, RaycastHit hit) = HasHitMesh();
        if (meshCollider != null)
        {
            //     Debug.Log("Collider found");
            Mesh mesh = meshCollider.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;

            int triIndex = hit.triangleIndex;

            int i0 = mesh.triangles[triIndex * 3 + 0];
            int i1 = mesh.triangles[triIndex * 3 + 1];
            int i2 = mesh.triangles[triIndex * 3 + 2];

            Vector3 v0 = vertices[i0];
            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];

            v0 = transform.TransformPoint(v0);
            v1 = transform.TransformPoint(v1);
            v2 = transform.TransformPoint(v2);

            Vector3 pos = (v0 + v1 + v2) / 3f;
            Debug.Log("Destroy");
            if (world.isPlacedEntityPresent(pos)) world.DestroyPlacedEntity(pos);

        }

    }

/*
    private void SelectEntity()
    {
        if (!isSelecting)
            return;

        (MeshCollider meshCollider, RaycastHit hit) = HasHitMesh();
        GameObject go = null;

        if (hit.collider != null)
        {
            try
            {
                // First: try selection based on the hit point
                Vector3 pos = hit.point;
                go = world.GetPlacedEntity(pos);

                // If nothing found, fall back to mesh triangle center
                if (go == null && meshCollider != null && meshCollider.sharedMesh.isReadable)
                {
                    Mesh mesh = meshCollider.sharedMesh;
                    int triIndex = hit.triangleIndex;

                    int i0 = mesh.triangles[triIndex * 3 + 0];
                    int i1 = mesh.triangles[triIndex * 3 + 1];
                    int i2 = mesh.triangles[triIndex * 3 + 2];

                    Vector3[] vertices = mesh.vertices;

                    Vector3 v0 = meshCollider.transform.TransformPoint(vertices[i0]);
                    Vector3 v1 = meshCollider.transform.TransformPoint(vertices[i1]);
                    Vector3 v2 = meshCollider.transform.TransformPoint(vertices[i2]);

                    pos = (v0 + v1 + v2) / 3f;

                    go = world.GetPlacedEntity(pos);
                }

                // If still nothing, fallback to the collider's GameObject
                if (go == null)
                    go = hit.collider.gameObject;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            Debug.Log("Select");

            if (go != null)
            {
                popupRoot.style.display = DisplayStyle.Flex;
                Label nameLbl = popupRoot.Q<Label>("nameLbl");

                EntityStats stats = go.GetComponent<EntityStats>();
                if (stats == null)
                {
                    popupRoot.style.display = DisplayStyle.None;
                    return;
                }

                nameLbl.text = stats.ToString();
            }
            else
            {
                popupRoot.style.display = DisplayStyle.None;
            }
        }
    }
*/
private void SelectEntity()
{
    if (!isSelecting)
        return;

    // Raycast that includes triggers
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;
    if (!Physics.Raycast(ray, out hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Collide))
    {
        popupRoot.style.display = DisplayStyle.None;
        return;
    }

    GameObject go = null;

    try
    {
        Vector3 pos = hit.point;

        // Try to get placed entity from world
        go = world.GetPlacedEntity(pos);
        if (go != null)
        {
            Debug.Log("Found at " + pos.ToString());
        }

        // Fallback: if nothing found, try mesh triangle center
                MeshCollider meshCollider = hit.collider as MeshCollider;
        if (go == null && meshCollider != null && meshCollider.sharedMesh != null && meshCollider.sharedMesh.isReadable)
        {
            int triIndex = hit.triangleIndex;
            Mesh mesh = meshCollider.sharedMesh;
            int[] tris = mesh.triangles;
            Vector3[] verts = mesh.vertices;

            Vector3 v0 = meshCollider.transform.TransformPoint(verts[tris[triIndex * 3]]);
            Vector3 v1 = meshCollider.transform.TransformPoint(verts[tris[triIndex * 3 + 1]]);
            Vector3 v2 = meshCollider.transform.TransformPoint(verts[tris[triIndex * 3 + 2]]);

            pos = (v0 + v1 + v2) / 3f;

            go = world.GetPlacedEntity(pos);
        }

        // Final fallback: use the collider itself
        go ??= hit.collider.gameObject;
    }
    catch (Exception e)
    {
        Debug.LogError(e);
        popupRoot.style.display = DisplayStyle.None;
        return;
    }

    if (go == null)
    {
        popupRoot.style.display = DisplayStyle.None;
        return;
    }

    // Show popup if it has EntityStats
    EntityStats stats = go.GetComponent<EntityStats>();
    if (stats == null)
    {
        popupRoot.style.display = DisplayStyle.None;
        return;
    }

    popupRoot.style.display = DisplayStyle.Flex;
    Label nameLbl = popupRoot.Q<Label>("nameLbl");
    nameLbl.text = stats.ToString();
}


    public void SetBuildOption(bool canBuild, int buildOption)
    {
        this.canBuild = canBuild;
        this.canDestroy = false;
        this.isSelecting = false;
        this.buildOption = (buildOption == -1) ? this.buildOption : buildOption;
    }

    public void SetDestroyOption(bool canDestroy)
    {
        this.buildOption = -1;
        this.canBuild = false;
        this.isSelecting = false;
        this.canDestroy = canDestroy;
    }

    public void SetSelectOption(bool isSelecting)
    {
        this.canBuild = false;
        this.canDestroy = false;
        this.buildOption = -1;
        this.isSelecting = isSelecting;
        if (!this.isSelecting)
        {
            popupRoot.style.display = DisplayStyle.None;
        }
    }
}
