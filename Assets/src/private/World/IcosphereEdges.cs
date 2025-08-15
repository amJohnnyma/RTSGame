using UnityEngine;

[ExecuteInEditMode]
public class IcosphereEdges : MonoBehaviour
{
    public MeshFilter meshFilter;
    public Color edgeColor = Color.black;

    private void OnDrawGizmos()
    {
        if (meshFilter == null || meshFilter.sharedMesh == null)
            return;

        Gizmos.color = edgeColor;
        Vector3[] verts = meshFilter.sharedMesh.vertices;
        int[] tris = meshFilter.sharedMesh.triangles;

        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector3 a = meshFilter.transform.TransformPoint(verts[tris[i]]);
            Vector3 b = meshFilter.transform.TransformPoint(verts[tris[i + 1]]);
            Vector3 c = meshFilter.transform.TransformPoint(verts[tris[i + 2]]);

            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, a);
        }
    }
}
