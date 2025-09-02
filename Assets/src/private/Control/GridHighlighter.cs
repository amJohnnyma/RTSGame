using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider), typeof(MeshFilter))]
public class GridHighlighter : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Color lineColor = Color.black;
    public Material lineMat;            // Will be created if left null
    public int patchSize = 25;          // Number of triangles to highlight in the patch
    public float lineOffset = 0.001f;   // Push lines off surface to avoid z-fighting

    [Header("Vertex Welding (for adjacency)")]
    [Tooltip("Positions within this distance are treated as the same vertex when building adjacency.")]
    public float weldTolerance = 1e-4f;

    private int hoverTriangle = -1;
    private int lastTriangle = -1;

    private Mesh mesh;
    private HashSet<int> currentPatch;

    private Mesh lineMesh;

    // Adjacency: for each triangle index -> list of neighboring triangle indices
    private List<int>[] adjacentTriangles;

    void Awake()
    {
        // Work on a clone so we don't mutate assets or mismatch collider/mesh
        mesh = Instantiate(GetComponent<MeshFilter>().mesh);
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        if (mesh.normals == null || mesh.normals.Length != mesh.vertexCount)
            mesh.RecalculateNormals();

        BuildAdjacencyWelded();

        // Auto-create a simple colored line material if none was provided
        if (lineMat == null)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMat = new Material(shader);
            lineMat.hideFlags = HideFlags.HideAndDontSave;
            lineMat.SetInt("_ZWrite", 0);
            lineMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        }
    }

    void Update()
    {
        // Raycast to find hovered triangle
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
        {
            hoverTriangle = hit.triangleIndex; // single submesh: this is global
        }
        else
        {
            hoverTriangle = -1;
        }

        // Only update patch if triangle changed
        if (hoverTriangle != lastTriangle)
        {
            currentPatch = (hoverTriangle >= 0) ? GetTrianglePatch(hoverTriangle, patchSize) : null;
            lastTriangle = hoverTriangle;

            // Rebuild the line mesh
            BuildLineMesh();
        }
    }

    void OnRenderObject()
    {
        if (lineMesh == null || lineMesh.vertexCount == 0) return;

        lineMat.SetPass(0);
        Graphics.DrawMeshNow(lineMesh, transform.localToWorldMatrix);
    }

    private HashSet<int> GetTrianglePatch(int start, int max)
    {
        var visited = new HashSet<int> { start };
        var q = new Queue<int>();
        q.Enqueue(start);

        while (q.Count > 0 && visited.Count < max)
        {
            int t = q.Dequeue();
            var neighbors = adjacentTriangles[t];
            for (int i = 0; i < neighbors.Count && visited.Count < max; i++)
            {
                int n = neighbors[i];
                if (visited.Add(n))
                    q.Enqueue(n);
            }
        }
        return visited;
    }

   
    private void BuildLineMesh()
    {
        if (currentPatch == null || currentPatch.Count == 0)
        {
            lineMesh = null;
            return;
        }

        List<Vector3> verts = new List<Vector3>();
        List<int> indices = new List<int>();

        int[] tris = mesh.triangles;
        Vector3[] vertsBase = mesh.vertices;
        Vector3[] norms = mesh.normals;

        foreach (int triIdx in currentPatch)
        {
            int baseIdx = triIdx * 3;
            int i0 = tris[baseIdx], i1 = tris[baseIdx + 1], i2 = tris[baseIdx + 2];
            Vector3 v0 = vertsBase[i0] + norms[i0] * lineOffset;
            Vector3 v1 = vertsBase[i1] + norms[i1] * lineOffset;
            Vector3 v2 = vertsBase[i2] + norms[i2] * lineOffset;

            int start = verts.Count;
            verts.Add(v0); verts.Add(v1); verts.Add(v2);

            indices.Add(start); indices.Add(start + 1);
            indices.Add(start + 1); indices.Add(start + 2);
            indices.Add(start + 2); indices.Add(start);
        }

        lineMesh = new Mesh();
        lineMesh.SetVertices(verts);
        lineMesh.SetIndices(indices, MeshTopology.Lines, 0);
    }

    private void BuildAdjacencyWelded()
    {
        int[] tris = mesh.triangles;
        Vector3[] verts = mesh.vertices;
        int triCount = tris.Length / 3;

        int[] weldedIndex = new int[verts.Length];
        var posToCanon = new Dictionary<Vector3, int>(new Vector3Comparer());
        int canonCount = 0;

        float tol = Mathf.Max(weldTolerance, 1e-7f);
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 key = Quantize(verts[i], tol);
            if (!posToCanon.TryGetValue(key, out int canon))
            {
                canon = canonCount++;
                posToCanon[key] = canon;
            }
            weldedIndex[i] = canon;
        }

        var edgeMap = new Dictionary<(int a, int b), List<int>>();

        for (int t = 0; t < triCount; t++)
        {
            int ia = tris[t * 3 + 0];
            int ib = tris[t * 3 + 1];
            int ic = tris[t * 3 + 2];

            int wa = weldedIndex[ia];
            int wb = weldedIndex[ib];
            int wc = weldedIndex[ic];

            AddWeldedEdge(edgeMap, wa, wb, t);
            AddWeldedEdge(edgeMap, wb, wc, t);
            AddWeldedEdge(edgeMap, wc, wa, t);
        }

        adjacentTriangles = new List<int>[triCount];
        for (int i = 0; i < triCount; i++) adjacentTriangles[i] = new List<int>(6);

        foreach (var kv in edgeMap)
        {
            List<int> connected = kv.Value;
            int count = connected.Count;
            if (count < 2) continue;

            for (int i = 0; i < count; i++)
            {
                int ti = connected[i];
                for (int j = i + 1; j < count; j++)
                {
                    int tj = connected[j];
                    adjacentTriangles[ti].Add(tj);
                    adjacentTriangles[tj].Add(ti);
                }
            }
        }

        for (int i = 0; i < triCount; i++)
        {
            var list = adjacentTriangles[i];
            if (list.Count > 1)
            {
                var set = new HashSet<int>(list);
                list.Clear();
                list.AddRange(set);
            }
        }
    }

    private static Vector3 Quantize(Vector3 v, float tol)
    {
        return new Vector3(
            Mathf.Round(v.x / tol) * tol,
            Mathf.Round(v.y / tol) * tol,
            Mathf.Round(v.z / tol) * tol
        );
    }

    private static void AddWeldedEdge(Dictionary<(int a, int b), List<int>> map, int v1, int v2, int triIdx)
    {
        if (v1 == v2) return;
        if (v2 < v1) { int tmp = v1; v1 = v2; v2 = tmp; }
        var key = (v1, v2);
        if (!map.TryGetValue(key, out var list))
        {
            list = new List<int>(2);
            map[key] = list;
        }
        list.Add(triIdx);
    }

    private class Vector3Comparer : IEqualityComparer<Vector3>
    {
        public bool Equals(Vector3 a, Vector3 b) => a.x == b.x && a.y == b.y && a.z == b.z;
        public int GetHashCode(Vector3 v)
        {
            unchecked
            {
                int h = 17;
                h = h * 31 + v.x.GetHashCode();
                h = h * 31 + v.y.GetHashCode();
                h = h * 31 + v.z.GetHashCode();
                return h;
            }
        }
    }

    public void setPatchRadius(int size)
    {
        patchSize = size * size;
    }
}

/*using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider), typeof(MeshFilter))]
public class GridHighlighter : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Color lineColor = Color.black;
    public Material lineMat;            // Will be created if left null
    public int patchSize = 25;          // Number of triangles to highlight in the patch
    public float lineOffset = 0.001f;   // Push lines off surface to avoid z-fighting

    [Header("Vertex Welding (for adjacency)")]
    [Tooltip("Positions within this distance are treated as the same vertex when building adjacency.")]
    public float weldTolerance = 1e-4f;

    private int hoverTriangle = -1;
    private int lastTriangle = -1;

    private Mesh mesh;
    private HashSet<int> currentPatch;

    // Adjacency: for each triangle index -> list of neighboring triangle indices
    private List<int>[] adjacentTriangles;

    void Awake()
    {
        // Work on a clone so we don't mutate assets or mismatch collider/mesh
        mesh = Instantiate(GetComponent<MeshFilter>().mesh);
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        if (mesh.normals == null || mesh.normals.Length != mesh.vertexCount)
            mesh.RecalculateNormals();

        BuildAdjacencyWelded();

        // Auto-create a simple colored line material if none was provided
        if (lineMat == null)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMat = new Material(shader);
            lineMat.hideFlags = HideFlags.HideAndDontSave;
            lineMat.SetInt("_ZWrite", 0);
            lineMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        }


    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
        {
            hoverTriangle = hit.triangleIndex; // single submesh: this is global
        }
        else
        {
            hoverTriangle = -1;
        }

        if (hoverTriangle != lastTriangle)
        {
            currentPatch = (hoverTriangle >= 0) ? GetTrianglePatch(hoverTriangle, patchSize) : null;
            lastTriangle = hoverTriangle;
            // Debug: Uncomment to verify expansion
            // Debug.Log($"Hover tri {hoverTriangle}, patch size: {currentPatch?.Count}");
        }
    }

    void OnRenderObject()
    {
        if (currentPatch == null || currentPatch.Count == 0) return;

        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);

        lineMat.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(lineColor);

        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        foreach (int triIdx in currentPatch)
        {
            int baseIdx = triIdx * 3;

            int i0 = triangles[baseIdx + 0];
            int i1 = triangles[baseIdx + 1];
            int i2 = triangles[baseIdx + 2];

            Vector3 v0 = vertices[i0] + normals[i0] * lineOffset;
            Vector3 v1 = vertices[i1] + normals[i1] * lineOffset;
            Vector3 v2 = vertices[i2] + normals[i2] * lineOffset;

            // Edges
            GL.Vertex(v0); GL.Vertex(v1);
            GL.Vertex(v1); GL.Vertex(v2);
            GL.Vertex(v2); GL.Vertex(v0);
        }

        GL.End();
        GL.PopMatrix();
    }

    // Breadth-first expansion to collect up to 'max' triangles
    private HashSet<int> GetTrianglePatch(int start, int max)
    {
        var visited = new HashSet<int> { start };
        var q = new Queue<int>();
        q.Enqueue(start);

        while (q.Count > 0 && visited.Count < max)
        {
            int t = q.Dequeue();
            var neighbors = adjacentTriangles[t];
            for (int i = 0; i < neighbors.Count && visited.Count < max; i++)
            {
                int n = neighbors[i];
                if (visited.Add(n))
                    q.Enqueue(n);
            }
        }
        return visited;
    }

    // ===== Adjacency Builder that "welds" vertices by position =====

    private void BuildAdjacencyWelded()
    {
        int[] tris = mesh.triangles;
        Vector3[] verts = mesh.vertices;
        int triCount = tris.Length / 3;

        // Map each original vertex index -> a "welded" canonical index based on position
        int[] weldedIndex = new int[verts.Length];

        // Quantize positions to a grid so identical/super-close points share the same key
        var posToCanon = new Dictionary<Vector3, int>(new Vector3Comparer());
        int canonCount = 0;

        float tol = Mathf.Max(weldTolerance, 1e-7f);

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 key = Quantize(verts[i], tol);
            if (!posToCanon.TryGetValue(key, out int canon))
            {
                canon = canonCount++;
                posToCanon[key] = canon;
            }
            weldedIndex[i] = canon;
        }

        // Build edge -> triangles map using WELDED vertex indices
        var edgeMap = new Dictionary<(int a, int b), List<int>>();

        for (int t = 0; t < triCount; t++)
        {
            int ia = tris[t * 3 + 0];
            int ib = tris[t * 3 + 1];
            int ic = tris[t * 3 + 2];

            int wa = weldedIndex[ia];
            int wb = weldedIndex[ib];
            int wc = weldedIndex[ic];

            AddWeldedEdge(edgeMap, wa, wb, t);
            AddWeldedEdge(edgeMap, wb, wc, t);
            AddWeldedEdge(edgeMap, wc, wa, t);
        }

        // Build adjacency lists
        adjacentTriangles = new List<int>[triCount];
        for (int i = 0; i < triCount; i++) adjacentTriangles[i] = new List<int>(6);

        foreach (var kv in edgeMap)
        {
            List<int> connected = kv.Value;
            int count = connected.Count;
            if (count < 2) continue;

            // Fully connect all triangles that share this welded edge
            for (int i = 0; i < count; i++)
            {
                int ti = connected[i];
                for (int j = i + 1; j < count; j++)
                {
                    int tj = connected[j];
                    adjacentTriangles[ti].Add(tj);
                    adjacentTriangles[tj].Add(ti);
                }
            }
        }

        // Optional: remove duplicates in adjacency (if edges shared by >2 tris)
        for (int i = 0; i < triCount; i++)
        {
            var list = adjacentTriangles[i];
            if (list.Count > 1)
            {
                // Deduplicate quickly
                var set = new HashSet<int>(list);
                list.Clear();
                list.AddRange(set);
            }
        }

        // Debug:
        // Debug.Log($"Adjacency built (welded). Tris: {triCount}, Welded verts: {canonCount}, Edges: {edgeMap.Count}");
    }

    private static Vector3 Quantize(Vector3 v, float tol)
    {
        // Round to the nearest multiple of tol; using floats but consistent due to rounding
        return new Vector3(
            Mathf.Round(v.x / tol) * tol,
            Mathf.Round(v.y / tol) * tol,
            Mathf.Round(v.z / tol) * tol
        );
    }

    private static void AddWeldedEdge(Dictionary<(int a, int b), List<int>> map, int v1, int v2, int triIdx)
    {
        if (v1 == v2) return; // degenerate
        if (v2 < v1) { int tmp = v1; v1 = v2; v2 = tmp; }
        var key = (v1, v2);

        if (!map.TryGetValue(key, out var list))
        {
            list = new List<int>(2);
            map[key] = list;
        }
        list.Add(triIdx);
    }

    // Custom comparer to use Vector3 as a dictionary key after quantization (exact equality)
    private class Vector3Comparer : IEqualityComparer<Vector3>
    {
        public bool Equals(Vector3 a, Vector3 b) => a.x == b.x && a.y == b.y && a.z == b.z;
        public int GetHashCode(Vector3 v)
        {
            unchecked
            {
                int h = 17;
                h = h * 31 + v.x.GetHashCode();
                h = h * 31 + v.y.GetHashCode();
                h = h * 31 + v.z.GetHashCode();
                return h;
            }
        }
    }

    public void setPatchRadius(int size)
    {
        patchSize = size * size;
    }
}
*/
