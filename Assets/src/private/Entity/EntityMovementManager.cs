using System.Threading.Tasks;
using UnityEngine;

public class EntityMovementManager : MonoBehaviour
{
    public EntityRuntime[] entities;

    void FixedUpdate()
    {

        Vector3[] positions = new Vector3[entities.Length];
        Vector3[] targetPositions = new Vector3[entities.Length];

        for (int i = 0; i < entities.Length; i++)
        {
            positions[i] = entities[i].transform.position;      // main thread copy
            targetPositions[i] = entities[i].target.transform.position;   // main thread copy
        }
        // --- Phase 1: Parallel computation of next tangent direction ---
        Parallel.For(0, entities.Length, i =>
        {
            var entity = entities[i];
            if (entity.target == null) return;

            // Sample nearby points
            var nearbyPoints = GetNearbyPoints(positions[i], entity.radius, entity.checkPoints);
            entity.lastNearbyPoints = nearbyPoints;

            // Score points toward target
            
            float bestScore = float.MinValue;
            int bestIdx = 0;
            for (int j = 0; j < nearbyPoints.Length; j++)
            {
                float score = 1f / ((nearbyPoints[j] - targetPositions[i]).sqrMagnitude+ 0.001f);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestIdx = j;
                }
            }
            /*
            // Compute weights
            float[] weights = new float[nearbyPoints.Length];
            float totalWeight = 0f;
            int bestIdx = 0;
            for (int j = 0; j < nearbyPoints.Length; j++)
            {
                // Higher score = closer to target
                float score = 1f / ((nearbyPoints[j] - targetPositions[i]).sqrMagnitude + 0.001f);
                weights[j] = score;
                totalWeight += score;
            }


            // Pick a random index based on weights
                // Thread-safe RNG
            System.Random rnd = new System.Random(i * 1000 + (int)System.DateTime.Now.Ticks);

            // Pick a random index based on weights
            float r = (float)(rnd.NextDouble() * totalWeight);
            float accum = 0f;
            int chosenIdx = 0;
            for (int j = 0; j < weights.Length; j++)
            {
                accum += weights[j];
                if (r <= accum)
                {
                    chosenIdx = j;
                    break;
                }
            }

            // Use chosenIdx as the next point
            bestIdx = chosenIdx;
            */
            entity.chosenScore = bestIdx;

        });

        // --- Phase 2: Main thread - raycast to terrain and move Rigidbody ---
        foreach (var entity in entities)
        {
            if (entity.target == null || entity.rb == null) continue;

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
        private Vector3[] GetNearbyPoints(Vector3 pos, float radius, int samples)
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
