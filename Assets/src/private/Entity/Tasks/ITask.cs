using UnityEngine;

public enum TaskType
{
    Move,
    Gather,
    Attack,
    Idle,
    Home
}



public interface ITask
{
    TaskType Type { get; }
    Vector3 TargetPos { get; }
    int Priority { get; }
    bool IsComplete { get; }
    bool HasStarted { get; }

    bool SetHasStarted { set; }
    bool SetIsComplete { set; }

    void UpdateTask(EntityRuntime entity, Vector3 entityPos, Vector3 targetPos, World world, float visionRadius);
}

public class Move : ITask
{
    public TaskType Type => TaskType.Move;
    public Vector3 TargetPos { get; private set; }
    public int Priority { get; private set; }
    public bool IsComplete { get; private set; }
    public bool HasStarted { get; private set; }


    public bool SetHasStarted { set => HasStarted = value; }
    public bool SetIsComplete { set => IsComplete = value; }

    public Move(Vector3 target, int priority = 1)
    {
        TargetPos = target;
        Priority = priority;
        IsComplete = false;
        HasStarted = false;
    }

    public void UpdateTask(EntityRuntime entity, Vector3 entityPos, Vector3 targetPos, World world, float visionRadius)
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
