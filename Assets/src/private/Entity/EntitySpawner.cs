using UnityEngine;
using System.Collections.Generic;

public class EntitySpawner : MonoBehaviour
{

    public List<Entity> presets;
    public GameObject spawnContainer;

    public void SpawnOnTerrain(Mesh terrainMesh, Transform terrainTransform)
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

            for (int i = 0; i < vertices.Length; i++)
            {
                // Convert to world space
                Vector3 worldPos = terrainTransform.TransformPoint(vertices[i]);
                Vector3 worldNormal = terrainTransform.TransformDirection(normals[i]);

                float h = Mathf.InverseLerp(minH, maxH, worldPos.magnitude);

                float slope = Vector3.Dot(worldNormal, worldPos.normalized);

                if (h >= preset.minHeight && h <= preset.maxHeight &&
                    slope >= preset.slopeThreshold &&
                    Random.value < preset.density)
                {
                    // Orientation aligned with surface
                    Quaternion rot = Quaternion.FromToRotation(Vector3.up, worldNormal);

                    // Instantiate
                    GameObject go = Instantiate(preset.prefab, worldPos, rot, spawnContainer.transform);

                    // === FLUSH PLACEMENT ===
                    // Try to offset based on prefab bounds
                    Collider col = go.GetComponentInChildren<Collider>();
                    float offset = 0f;

                    if (col != null)
                    {
                        offset = col.bounds.extents.y; // half height
                    }
                    else
                    {
                        Renderer rend = go.GetComponentInChildren<Renderer>();
                        if (rend != null)
                            offset = rend.bounds.extents.y;
                    }

                    // Move slightly above the surface
                    go.transform.position += worldNormal * offset;
                }
            }

        }
    }


    
}