using System.Collections.Generic;
using UnityEngine;

public static class IcosphereGenerator
{
    public static Mesh Create(float radius, int subdivisions)
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

        List<int> faces = new List<int>
        {
            0, 11, 5,    0, 5, 1,    0, 1, 7,    0, 7, 10,    0, 10, 11,
            1, 5, 9,     5, 11, 4,   11, 10, 2,  10, 7, 6,    7, 1, 8,
            3, 9, 4,     3, 4, 2,    3, 2, 6,    3, 6, 8,     3, 8, 9,
            4, 9, 5,     2, 4, 11,   6, 2, 10,   8, 6, 7,     9, 8, 1
        };

        Dictionary<long, int> midpointCache = new Dictionary<long, int>();

        for (int s = 0; s < subdivisions; s++)
        {
            List<int> newFaces = new List<int>();
            for (int i = 0; i < faces.Count; i += 3)
            {
                int a = faces[i];
                int b = faces[i + 1];
                int c = faces[i + 2];

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

        return mesh;
    }

    private static int GetMidpoint(int i1, int i2, List<Vector3> verts, Dictionary<long, int> cache, float radius)
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
}
