using UnityEngine;
using System.Collections.Generic;
public interface IEntityBehaviour
{
    void ComputeMove(
        EntityRuntime entity,
        Vector3 entityPosition,
        Vector3 targetPosition,
        ITask task
        );

        void OnTargetReached(EntityRuntime entity, ITask task);

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

    public void ComputeMove(EntityRuntime entity, Vector3 entityPos, Vector3 targetPos, ITask task)
    {
        task.UpdateTask(entity, entityPos, targetPos, world, visionRadius);
        
    }

    public void OnTargetReached(EntityRuntime entity, ITask task)
    {
        task.OnTargetReached(entity, world);
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


    public void ComputeMove(EntityRuntime entity, Vector3 entityPos, Vector3 targetPos, ITask task)
    {

        task.UpdateTask(entity, entityPos, targetPos, world, visionRadius);
 
    }

     public void OnTargetReached(EntityRuntime entity, ITask task)
    {
        task.OnTargetReached(entity, world);

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