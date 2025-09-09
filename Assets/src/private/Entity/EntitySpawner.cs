using UnityEngine;
using System.Collections.Generic;

public class EntitySpawner : MonoBehaviour
{

    public List<Entity> presets;
    public GameObject spawnContainer;

    public void SpawnOnTerrain(Mesh terrainMesh, Transform terrainTransform, World world)
    {
        Debug.Log("Spawn on terrain");

        if (spawnContainer == null)
        {
            return;
        }

        for (int i = spawnContainer.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(spawnContainer.transform.GetChild(i).gameObject);
        }

        Vector3[] vertices = terrainMesh.vertices;
        Vector3[] normals = terrainMesh.normals;
        int[] triangles = terrainMesh.triangles;

        float maxH = float.MinValue;
        float minH = float.MaxValue;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = terrainTransform.TransformPoint(vertices[i]);
            float h = worldPos.magnitude;
            if (h < minH) minH = h;
            if (h > maxH) maxH = h;

        }
        Debug.Log($"Min height (h): {minH}, Max height (h): {maxH}");
        

        foreach (var preset in presets)
        {

            //iterate over tries to find idx
            for (int tri = 0; tri < triangles.Length; tri += 3)
            {
                int i0 = triangles[tri + 0];
                int i1 = triangles[tri + 1];
                int i2 = triangles[tri + 2];

                Vector3 v0 = terrainTransform.TransformPoint(vertices[i0]);
                Vector3 v1 = terrainTransform.TransformPoint(vertices[i1]);
                Vector3 v2 = terrainTransform.TransformPoint(vertices[i2]);

                Vector3 pos = (v0 + v1 + v2) / 3f;

                if (world.isPlacedEntityPresent(pos)) continue;

                // Interpolate normal across triangle
                Vector3 n0 = terrainTransform.TransformDirection(normals[i0]);
                Vector3 n1 = terrainTransform.TransformDirection(normals[i1]);
                Vector3 n2 = terrainTransform.TransformDirection(normals[i2]);
                Vector3 normal = (n0 + n1 + n2).normalized;

                float h = Mathf.InverseLerp(minH, maxH, pos.magnitude);
                float slope = Vector3.Dot(normal, pos.normalized);

                if (h >= preset.minHeight && h <= preset.maxHeight &&
                    slope >= preset.slopeThreshold &&
                    Random.value < preset.density)
                {
                    // Orientation aligned with surface
                    Quaternion rot = Quaternion.FromToRotation(Vector3.up, normal);

                    // Instantiate
                    GameObject go = Instantiate(preset.prefab, pos, rot);
                    go.transform.SetParent(spawnContainer.transform, true);
                    //go.transform.localScale = Vector3.one;
                    world.AddPlacedEntity(pos, go);

                    // Offset based on collider or renderer
                    float offset = 0f;
                    Collider col = go.GetComponentInChildren<Collider>();
                    if (col != null)
                        offset = col.bounds.extents.y;
                    else
                    {
                        Renderer rend = go.GetComponentInChildren<Renderer>();
                        if (rend != null)
                            offset = rend.bounds.extents.y;
                    }

                   // go.transform.position += normal * offset;
                }
            }

        }
    }


    
}