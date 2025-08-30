using System.Collections.Generic;
using UnityEngine;

public class IcosphereGenerator
{
    
    public List<Chunk> Create(float radius, int subdivisions, GameObject sphere)
    {

        // === Base icosahedron ===
        float t = (1f + Mathf.Sqrt(5f)) / 2f;

        List<Vector3> verts = new List<Vector3>
        {
            new Vector3(-1,  t,  0),
            new Vector3( 1,  t,  0),
            new Vector3(-1, -t,  0),
            new Vector3( 1, -t,  0),

            new Vector3( 0, -1,  t),
            new Vector3( 0,  1,  t),
            new Vector3( 0, -1, -t),
            new Vector3( 0,  1, -t),

            new Vector3( t,  0, -1),
            new Vector3( t,  0,  1),
            new Vector3(-t,  0, -1),
            new Vector3(-t,  0,  1)
        };

        for (int i = 0; i < verts.Count; i++)
            verts[i] = verts[i].normalized * radius;

        int[] faces =
        {
            0, 11, 5,    0, 5, 1,    0, 1, 7,    0, 7, 10,    0, 10, 11,
            1, 5, 9,     5, 11, 4,   11, 10, 2,  10, 7, 6,    7, 1, 8,
            3, 9, 4,     3, 4, 2,    3, 2, 6,    3, 6, 8,     3, 8, 9,
            4, 9, 5,     2, 4, 11,   6, 2, 10,   8, 6, 7,     9, 8, 1
        };

        List<Chunk> chunks = new List<Chunk>();

        for (int i = 0; i < faces.Length; i += 3)
        {
            Chunk c = MakeChunk(verts[faces[i]], verts[faces[i + 1]], verts[faces[i + 2]], 0);
            c.go.transform.SetParent(sphere.transform, false);

            chunks.Add(c);
        }

        foreach (Chunk c in chunks)
        {
            RecSubdivide(c, radius, subdivisions, subdivisions);
            
        }

        return chunks;
        /*
                                        Dictionary<long, int> midpointCache = new Dictionary<long, int>();

                                        for (int s = 0; s < subdivisions; s++)
                                        {
                                            List<int> newFaces = new List<int>();
                                            for (int i = 0; i < faces.Count; i += 3)
                                            {
                                                int a = faces[i+2];
                                                int b = faces[i + 1];
                                                int c = faces[i + 0];

                                                int ab = GetMidpoint(a, b, verts, midpointCache, radius);
                                                int bc = GetMidpoint(b, c, verts, midpointCache, radius);
                                                int ca = GetMidpoint(c, a, verts, midpointCache, radius);

                                                newFaces.AddRange(new int[] { a, ab, ca, b, bc, ab, c, ca, bc, ab, bc, ca });
                                            }
                                            faces = newFaces;
                                        }

                                        Vector2[] uvs = new Vector2[verts.Count];
                                        for (int i = 0; i < verts.Count; i++)
                                        {
                                            Vector3 v = verts[i].normalized;
                                            float u = Mathf.Atan2(v.x, v.z) / (2 * Mathf.PI) + 0.5f;
                                            float vv = v.y * 0.5f + 0.5f;
                                            uvs[i] = new Vector2(u, vv);
                                        }

                                        for (int i = 0; i < faces.Count; i += 3)
                                        {
                                            int temp = faces[i + 1];
                                            faces[i + 1] = faces[i + 2];
                                            faces[i + 2] = temp;
                                        }

                                        Mesh mesh = new Mesh();
                                        mesh.name = "Icosphere";
                                        mesh.vertices = verts.ToArray();
                                        mesh.triangles = faces.ToArray();
                                        mesh.uv = uvs;
                                        mesh.RecalculateNormals();

                                        return mesh
                                    */
    }

    private int GetMidpoint(int i1, int i2, List<Vector3> verts, Dictionary<long, int> cache, float radius)
    {
        long key = ((long)Mathf.Min(i1, i2) << 32) + Mathf.Max(i1, i2);
        if (cache.TryGetValue(key, out int ret))
            return ret;

        Vector3 mid = ((verts[i1] + verts[i2]) * 0.5f).normalized * radius;
        int index = verts.Count;
        verts.Add(mid);
        cache.Add(key, index);

        return index;
    }

    private Mesh MakeMesh(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        Mesh m = new Mesh();
        m.vertices = new Vector3[] { v1, v2, v3 };
        m.triangles = new int[] { 0, 1, 2 };
        m.RecalculateNormals();
        return m;
    }
    public void Subdivide(Chunk c, float radius, int maxdepth)
    {
        if (c.depth >= maxdepth) return;
        
        Vector3 m1 = ((c.v1 + c.v2) * 0.5f).normalized * radius;
        Vector3 m2 = ((c.v2 + c.v3) * 0.5f).normalized * radius;
        Vector3 m3 = ((c.v3 + c.v1) * 0.5f).normalized * radius;

        c.children = new Chunk[4];
        c.children[0] = MakeChunk(c.v1, m1, m3, c.depth + 1);
        c.children[0].go.transform.SetParent(c.go.transform, false);
        c.children[1] = MakeChunk(c.v2, m2, m1, c.depth + 1);
        c.children[1].go.transform.SetParent(c.go.transform, false);
        c.children[2] = MakeChunk(c.v3, m3, m2, c.depth + 1);
        c.children[2].go.transform.SetParent(c.go.transform, false);
        c.children[3] = MakeChunk(m1, m2, m3, c.depth + 1);
        c.children[3].go.transform.SetParent(c.go.transform, false);

        c.isLeaf = false;
        c.isSubdivided = true;
        c.go.SetActive(false);
    }

    public void RecSubdivide(Chunk c, float radius, int depth, int max)
    {
        if (depth <= 0 || c.depth >= max)
        {
            return;
        }

        Subdivide(c, radius, max);

        foreach (Chunk child in c.children)
        {
            RecSubdivide(child, radius, depth - 1, max);
        }
    }

    private Chunk MakeChunk(Vector3 v1, Vector3 v2, Vector3 v3, int depth)
    {
        Chunk c = new Chunk();
        c.depth = depth;
        c.v1 = v1; c.v2 = v2; c.v3 = v3;
        c.mesh = MakeMesh(v1, v2, v3);
        c.go = new GameObject("Chunk");
        c.go.AddComponent<MeshFilter>().mesh = c.mesh;
        c.go.AddComponent<MeshRenderer>();
        List<Material> loadedMaterial = new List<Material>
        {
            Resources.Load<Material>("src/Materials/WorldMat")
        };
        c.go.GetComponent<MeshRenderer>().SetMaterials(loadedMaterial);
        return c;
    }

    public int[] Collapse(Chunk c)
    {
        if (!c.isSubdivided)
        {
            return new int[0];
        }

        List<int> del = new List<int>();
        for (int i = 0; i < c.children.Length; i++)
        {
            if (c.children[i] != null)
            {
                del.Add(i);
            }
        }


        return del.ToArray();
    }
}
