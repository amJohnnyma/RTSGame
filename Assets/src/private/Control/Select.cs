using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Select : MonoBehaviour
{

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
}
