using UnityEngine;
using System.Collections.Generic;

public class EntitySpawner : MonoBehaviour
{

    public List<Entity> presets;
    private GameObject spawnContainer;

    public void SpawnOnTerrain(Mesh terrainMesh)
    {
        Debug.Log("Spawn on terrain");


        // Destroy old container if it exists
        if (spawnContainer != null)
        {
            DestroyImmediate(spawnContainer); // or Destroy() if called at runtime
        }

        // Create a new container
        spawnContainer = new GameObject("SpawnContainer");
        spawnContainer.transform.parent = transform;

        Vector3[] vertices = terrainMesh.vertices;
        Vector3[] normals = terrainMesh.normals;

        foreach (var preset in presets)
        {
            //temp
            float minH = float.MaxValue;
            float maxH = float.MinValue;
            //
            for (int i = 0; i < vertices.Length; i++)
            {
                float h = vertices[i].magnitude; // height above sphere radius
                if (h < minH) minH = h;
                if (h > maxH) maxH = h;
                float slope = Vector3.Dot(normals[i], vertices[i].normalized);


                if (h >= (preset.minHeight) && h <= (preset.maxHeight) &&
                    slope >= preset.slopeThreshold &&
                    Random.value < preset.density)
                {
                    Vector3 pos = vertices[i];
                    Quaternion rot = Quaternion.FromToRotation(Vector3.up, normals[i]);

                    // Parent new instances under container
                    Instantiate(preset.prefab, pos, rot, spawnContainer.transform);
                }
            }
            Debug.Log($"Min height (h): {minH}, Max height (h): {maxH}");
        }
    }
}