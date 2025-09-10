using UnityEngine;
using System.Collections.Generic;
public interface IEntityBehaviour
{
    void ComputeMove(
        EntityRuntime entity,
        Vector3 entityPosition,
        Vector3 targetPosition
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



    public ScoutBehavior(World world, float visionRadius = 1f)
    {
        this.world = world;
        this.visionRadius = visionRadius * 4;
    }

    public void ComputeMove(EntityRuntime entity, Vector3 entityPos, Vector3 targetPos)
    {
        if (!entity.returningHome && world.GetFoundHarvestable(targetPos))
        {
        // Current target is stale, force scout to re-pick
        entity.pendingTargetPos = Vector3.positiveInfinity;
        }


        if (!entity.returningHome)
        {
            // --- Step 1: Pure data harvestable check ---
            var harvestables = EntitySpatialUtils.GetNearbyHarvestables(world.GetUnfoundHarvestableSnapshot(), entityPos, visionRadius, world);
            if (harvestables.Count > 0)
            {
                Vector3 bestHarvest = harvestables[0];
                float bestDist = (bestHarvest - entityPos).sqrMagnitude;

                for (int i = 1; i < harvestables.Count; i++)
                {
                    float dist = (harvestables[i] - entityPos).sqrMagnitude;
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestHarvest = harvestables[i];
                    }
                }

                // Switch if closer than current target
                if (bestDist < (targetPos - entityPos).sqrMagnitude || targetPos == Vector3.positiveInfinity)
                {
                    entity.pendingTargetPos = bestHarvest;
                }
            }
        }


        // --- Step 2: Movement scoring (still parallel safe) ---
        var nearbyPoints = EntityMovementManager.GetNearbyPoints(entityPos, entity.radius, entity.checkPoints);
        entity.lastNearbyPoints = nearbyPoints;

        float bestScore = float.MinValue;
        int bestIdx = 0;
        for (int j = 0; j < nearbyPoints.Length; j++)
        {
            float score = 1f / ((nearbyPoints[j] - targetPos).sqrMagnitude + 0.001f);
            if (score > bestScore)
            {
                bestScore = score;
                bestIdx = j;
            }
        }

        entity.chosenScore = bestIdx;


    }
}

public class HarvestBehaviour : IEntityBehaviour
{

    private World world;
    private float visionRadius;



    public HarvestBehaviour(World world, float visionRadius = 1f)
    {
        this.world = world;
        this.visionRadius = visionRadius * 4;
    }


    public void ComputeMove(EntityRuntime entity, Vector3 entityPos, Vector3 targetPos)
    {

        if (!entity.returningHome)
        {
            // --- Step 1: Pure data harvestable check ---
            var harvestables = EntitySpatialUtils.GetNearbyHarvestables(world.GetUnfoundHarvestableSnapshot(), entityPos, visionRadius, world);
            if (harvestables.Count > 0)
            {
                Vector3 bestHarvest = harvestables[0];
                float bestDist = (bestHarvest - entityPos).sqrMagnitude;

                for (int i = 1; i < harvestables.Count; i++)
                {
                    float dist = (harvestables[i] - entityPos).sqrMagnitude;
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestHarvest = harvestables[i];
                    }
                }

                // Switch if closer than current target
                if (bestDist < (targetPos - entityPos).sqrMagnitude || targetPos == Vector3.positiveInfinity)
                {
                    entity.pendingTargetPos = bestHarvest;
                }
            }
        }

        // --- Step 2: Movement scoring (still parallel safe) ---
        var nearbyPoints = EntityMovementManager.GetNearbyPoints(entityPos, entity.radius, entity.checkPoints);
        entity.lastNearbyPoints = nearbyPoints;

        float bestScore = float.MinValue;
        int bestIdx = 0;
        for (int j = 0; j < nearbyPoints.Length; j++)
        {
            float score = 1f / ((nearbyPoints[j] - targetPos).sqrMagnitude + 0.001f);
            if (score > bestScore)
            {
                bestScore = score;
                bestIdx = j;
            }
        }

        entity.chosenScore = bestIdx;


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