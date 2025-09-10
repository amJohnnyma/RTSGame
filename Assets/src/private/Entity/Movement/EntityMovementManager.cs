using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EntityMovementManager : MonoBehaviour
{
    public EntityRuntime[] entities;

    private World world;

    void Start()
    {
        world = GetComponent<World>();
    }

    void FixedUpdate()
    {
        world.RefreshHarvestableCache();

        Vector3[] positions = new Vector3[entities.Length];
        Vector3[] targetPositions = new Vector3[entities.Length];

        for (int i = 0; i < entities.Length; i++)
        {
            positions[i] = entities[i].transform.position;      // main thread copy
            targetPositions[i] = entities[i].target == null ? entities[i].home.transform.position : entities[i].target.transform.position;   // main thread copy
        }
        // --- Phase 1: Parallel computation of next tangent direction ---
        Parallel.For(0, entities.Length, i =>
        {
            var entity = entities[i];
            if (entity.target == null) return;

            entity.behaviorHandler.ComputeMove(entity, positions[i], targetPositions[i]);

        });

        // --- Phase 2: Main thread - raycast to terrain and move Rigidbody ---
        foreach (var entity in entities)
        {
            if (entity.target == null || entity.rb == null) continue;

            if (entity.pendingTargetPos != Vector3.positiveInfinity)
            {
                if (world.placedEntities.TryGetValue(entity.pendingTargetPos, out var go))
                {
                    entity.target = go.transform;
                }
                entity.pendingTargetPos = Vector3.positiveInfinity; // reset
            }

            if ((entity.target.position - entity.transform.position).sqrMagnitude < entity.stopFollowDist * entity.stopFollowDist)
            {
                entity.rb.velocity = Vector3.zero;
                entity.SetTargetToggle();
                continue;
            }

            Vector3 downDir = -entity.transform.up;
            if (Physics.Raycast(entity.transform.position + downDir * 0.1f, downDir,
                                out RaycastHit hit, entity.raycastDownDist, 1 << entity.worldCollider.gameObject.layer))
            {
                Vector3 groundPoint = hit.point;
                Vector3 groundNormal = hit.normal;

                Vector3 moveVec = entity.lastNearbyPoints[entity.chosenScore] - entity.transform.position;
                Vector3 tangentDir = Vector3.ProjectOnPlane(moveVec, hit.normal);
                if (tangentDir.sqrMagnitude < 0.0001f)
                {
                    tangentDir = Vector3.Cross(hit.normal, Vector3.forward).normalized; // fallback tangent
                }
                else
                {
                    tangentDir.Normalize();
                }


                // Apply velocity along tangent (from parallel calculation)
                Vector3 desiredVel = tangentDir * entity.moveSpeed;
                entity.rb.velocity = Vector3.Lerp(entity.rb.velocity, desiredVel, Time.fixedDeltaTime * entity.turnSpeed);

                float maxSpeed = entity.moveSpeed * 1.5f; // safety buffer
                if (entity.rb.velocity.magnitude > maxSpeed)
                    entity.rb.velocity = entity.rb.velocity.normalized * maxSpeed;


                // Stick to terrain surface
                entity.rb.MovePosition(groundPoint + groundNormal * entity.surfaceOffset);

                // Align rotation with terrain
                Quaternion targetRot = Quaternion.FromToRotation(entity.transform.up, groundNormal) * entity.transform.rotation;
                entity.rb.MoveRotation(Quaternion.Slerp(entity.rb.rotation, targetRot, Time.fixedDeltaTime * entity.turnSpeed));
                entity.rb.AddForce(-entity.transform.up * 20f, ForceMode.Acceleration);

            }
            else
            {
                entity.rb.velocity = Vector3.zero;
            }
        }
    }

    // --- Utility: Fibonacci sphere sampling ---
        public static Vector3[] GetNearbyPoints(Vector3 pos, float radius, int samples)
        {
            Vector3[] points = new Vector3[samples];
            float phi = Mathf.PI * (3f - Mathf.Sqrt(5f));

            for (int i = 0; i < samples; i++)
            {
                float y = 1f - (i / (float)(samples - 1)) * 2f;
                float r = Mathf.Sqrt(1 - y * y);
                float theta = phi * i;
                float x = Mathf.Cos(theta) * r;
                float z = Mathf.Sin(theta) * r;
                points[i] = pos + new Vector3(x, y, z) * radius;
            }

            return points;
        }


}

public static class EntitySpatialUtils
{
    // Thread-safe: return nearby **positions only**
    public static List<Vector3> GetNearbyEntities(Vector3 self, float radius, Vector3[] allPositions)
    {
        List<Vector3> nearby = new List<Vector3>();
        float r2 = radius * radius;

        foreach (var pos in allPositions)
        {
            if (pos == self) continue;
            if ((pos - self).sqrMagnitude <= r2)
                nearby.Add(pos);
        }
        return nearby;
    }

    // Thread-safe harvestable detection using positions
    public static List<Vector3> GetNearbyHarvestables(IReadOnlyList<Vector3> harvestablePositions, Vector3 pos, float radius, World world)
    {
        List<Vector3> nearby = new List<Vector3>();
        float r2 = radius * radius;

        for (int i = 0; i < harvestablePositions.Count; i++)
        {
            var entityPos = harvestablePositions[i];
            if (world.GetFoundHarvestable(entityPos)) continue;
            if ((entityPos - pos).sqrMagnitude <= r2)
                nearby.Add(entityPos);
        }

        return nearby;
    }

}