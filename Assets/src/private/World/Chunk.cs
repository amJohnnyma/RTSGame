using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public Vector3 v1, v2, v3;
    public int depth;
    public Mesh mesh;
    public GameObject go;
    public Chunk[] children; //subdivided children
    public bool isLeaf = true;
    public bool isSubdivided = false;

    public Vector3 Center() => (v1 + v2 + v3) / 3f;
    public void Activate(bool b)
    {
        go.SetActive(b);
        if (isLeaf) return;
        
        foreach (Chunk c in children)
        {
            c.Activate(b);
        }
    }

}
