using UnityEngine;
using System.Collections.Generic;
public interface IEntityBehaviour
{
    Vector3 ComputeMove(
        EntityRuntime entity,
        Vector3 entityPosition,
        Vector3 targetPosition,
        Vector3[] allPositions
        );

}

/*
public class DefaultBehavior : IEntityBehaviour
{
    public Vector3 ComputeMove(EntityRuntime entity, Vector3 entityPos, Vector3 targetPos, Vector3[] allPositions)
    {
        // Simple: move towards target
        var nearbyPoints = EntityMovementManager.GetNearbyPointsStatic(entityPos, entity.radius, entity.checkPoints);
        entity.lastNearbyPoints = nearbyPoints;

        float bestScore = float.MinValue;
        int bestIdx = 0;

        for (int i = 0; i < nearbyPoints.Length; i++)
        {
            float score = 1f / ((nearbyPoints[i] - targetPos).sqrMagnitude + 0.001f);
            if (score > bestScore)
            {
                bestScore = score;
                bestIdx = i;
            }
        }

        entity.chosenScore = bestIdx;
        return nearbyPoints[bestIdx] - entityPos;
    }


}

public class WanderBehavior : IEntityBehaviour
{
    public Vector3 ComputeMove(EntityRuntime entity, Vector3 entityPos, Vector3 targetPos, Vector3[] allPositions)
    {
        // Random wandering
        var nearbyPoints = EntityMovementManager.GetNearbyPointsStatic(entityPos, entity.radius, entity.checkPoints);
        entity.lastNearbyPoints = nearbyPoints;
        int randomIdx = Random.Range(0, nearbyPoints.Length);
        entity.chosenScore = randomIdx;
        return nearbyPoints[randomIdx] - entityPos;
    }


}
*/
public class ScoutBehavior : IEntityBehaviour
{
    private World world;
    private float visionRadius;

    // Keep a persistent target harvestable
    private EntityStats currentTargetHarvestable = null;

    // Keep a persistent random wander point
    private Vector3 persistentWanderPoint;

    private float wanderPointUpdateInterval = 10f; // seconds
    private float wanderTimer = 0f;

    public ScoutBehavior(World world, float visionRadius = 1f)
    {
        this.world = world;
        this.visionRadius = visionRadius * 4;
        this.persistentWanderPoint = Vector3.zero;
    }

    public Vector3 ComputeMove(EntityRuntime entity, Vector3 entityPos, Vector3 targetPos, Vector3[] allPositions)
    {
        /*
        // --- Phase 1: Get nearby points for movement ---
        Vector3[] nearbyPoints = EntityMovementManager.GetNearbyPointsStatic(entityPos, entity.radius, entity.checkPoints);
        entity.lastNearbyPoints = nearbyPoints;


        // --- Phase 3: Detect nearby harvestables (Vector3 positions only) ---
        List<Vector3> harvestables = EntitySpatialUtils.GetNearbyHarvestables(world, entityPos, entity.radius * 2f);

        // --- Phase 4: Choose target point ---
        Vector3 targetPoint = persistentWanderPoint;

        // If there is a harvestable nearby, move toward the closest
        if (harvestables.Count > 0)
        {
            float closestDist = float.MaxValue;
            foreach (var hPos in harvestables)
            {
                float dist = (hPos - entityPos).sqrMagnitude;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    targetPoint = hPos;
                }
            }
        }

        // --- Phase 5: Score nearby points and avoid neighbors ---
        float bestScore = float.MinValue;
        int bestIdx = 0;

        List<Vector3> neighbors = EntitySpatialUtils.GetNearbyEntities(entityPos, entity.radius, allPositions);

        for (int i = 0; i < nearbyPoints.Length; i++)
        {
            Vector3 point = nearbyPoints[i];
            float score = 1f / ((point - targetPoint).sqrMagnitude + 0.001f);

            // Avoid neighbors
            foreach (var n in neighbors)
            {
                score -= 1f / ((point - n).sqrMagnitude + 0.001f);
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestIdx = i;
            }
        }

        entity.chosenScore = bestIdx;

        // Return the vector pointing to the chosen point
        return nearbyPoints[bestIdx] - entityPos;
        */
        return default;
    }
}

/*
public class AttackBehavior : IEntityBehaviour
{
    public Vector3 ComputeMove(EntityRuntime entity, Vector3 entityPos, Vector3 targetPos, Vector3[] allPositions)
    {
        var nearbyPoints = EntityMovementManager.GetNearbyPointsStatic(entityPos, entity.radius, entity.checkPoints);
        entity.lastNearbyPoints = nearbyPoints;

        // Find nearby entities (potential enemies)
        List<Vector3> enemies = EntitySpatialUtils.GetNearbyEntities(entity.transform.position, entity.radius, allPositions);

        float bestScore = float.MinValue;
        int bestIdx = 0;

        for (int i = 0; i < nearbyPoints.Length; i++)
        {
            Vector3 point = nearbyPoints[i];

            // Default: move towards main target
            float score = 1f / ((point - targetPos).sqrMagnitude + 0.001f);

            // If there are enemies, prioritize points closer to them
            foreach (var e in enemies)
            {
                float dist = (point - e).sqrMagnitude;
                score += 1f / (dist + 0.001f); // increase score if point is close to enemy
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestIdx = i;
            }
        }

        entity.chosenScore = bestIdx;
        return nearbyPoints[bestIdx] - entityPos;
    }
}
*/