using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Select : MonoBehaviour
{

    public List<GameObject> presets;


    private bool canBuild;
    private int buildOption = -1;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        CheckInput();
    }

    private void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Key down");
            RayHitMesh();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Alpha one down");
            SpawnEntity();
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

            Vector3 pos = hit.point;
            // Interpolate normal across the triangle instead of picking one vertex
            Vector3 baryCenter = hit.barycentricCoordinate;
            Vector3 normal =
                -normals[i0] * baryCenter.x +
                -normals[i1] * baryCenter.y +
                -normals[i2] * baryCenter.z;
            Quaternion rot = Quaternion.FromToRotation(normal, normal.normalized);

            Debug.Log("Instantiate");
            // Parent new instances under container`
            Instantiate(presets[buildOption-1], pos, rot, gameObject.transform);

        }
        
    }

    public void SetBuildOption(bool canBuild, int buildOption)
    {
        this.canBuild = canBuild;
        this.buildOption = (buildOption == -1) ? this.buildOption : buildOption;
    }
}
