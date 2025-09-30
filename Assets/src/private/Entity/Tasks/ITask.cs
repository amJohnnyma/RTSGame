using UnityEngine;

public enum TaskType
{
    Scout,
    Harvest,
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

    void OnTargetReached(EntityRuntime entity, World world);


    
}

public class Scout : ITask
{
    public TaskType Type => TaskType.Scout;
    public Vector3 TargetPos { get; private set; }
    public int Priority { get; private set; }
    public bool IsComplete { get; private set; }
    public bool HasStarted { get; private set; }


    public bool SetHasStarted { set => HasStarted = value; }
    public bool SetIsComplete { set => IsComplete = value; }

    public Scout(Vector3 target, int priority = 1)
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
            // Step 1: Pure data harvestable check
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

    public void OnTargetReached(EntityRuntime entity, World world)
    {
        if (entity.returningHome)
        {
            // Arrived home, now pick next target harvestable or wander
            EntityStats nextHarvestable = world.GetRandomPlacedHarvestable().GetComponent<EntityStats>();
            if (nextHarvestable != null)
            {
                entity.mainTarget = nextHarvestable.transform;
                entity.target = entity.mainTarget;
                entity.returningHome = false;
            }
            else
            {
                // No harvestables, pick random wander point
                entity.target = entity.mainTarget; // default to mainTarget for wandering
                entity.returningHome = false;
            }

            IsComplete = true; // Task done

            world.AddFoundHarvestable(entity.target.transform.position, entity.target.gameObject);


        }
        else
        {
            // Arrived at target (harvestable), go home next
            entity.target = entity.home;
            entity.returningHome = true;
        }


        
    }


}

public class Harvest : ITask
{
    public TaskType Type => TaskType.Scout;
    public Vector3 TargetPos { get; private set; }
    public int Priority { get; private set; }
    public bool IsComplete { get; private set; }
    public bool HasStarted { get; private set; }


    public bool SetHasStarted { set => HasStarted = value; }
    public bool SetIsComplete { set => IsComplete = value; }

    public Harvest(Vector3 target, int priority = 1)
    {
        TargetPos = target;
        Priority = priority;
        IsComplete = false;
        HasStarted = false;
    }

    public void UpdateTask(EntityRuntime entity, Vector3 entityPos, Vector3 targetPos, World world, float visionRadius)
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

    public void OnTargetReached(EntityRuntime entity, World world)
    {
        string harvestItem = "Red_Flower";
        int takeAmount = 1;
        int giveAmount = 1;

        switch (entity.behaviour)
        {
            case EntityBehaviour.SCOUT:
                takeAmount = 1;
                giveAmount = 1;
                break;
            case EntityBehaviour.HARVEST:
                takeAmount = 10;
                giveAmount = int.MaxValue;
                break;
            case EntityBehaviour.DEFAULT:
                break;
        }
        if (entity.returningHome)
        {
            // Arrived home → deliver items
            var homeInv = entity.home.GetComponent<Inventory>();
            entity.GetComponent<Inventory>().GiveItemToOther(harvestItem, takeAmount, homeInv);

            // Pick next harvestable
            GameObject go = world.GetRandomFoundHarvestable();
            if (go != null)
            {
                entity.mainTarget = go.transform;
                entity.target = entity.mainTarget;
                entity.returningHome = false;
            }

            IsComplete = true;
        }
        else
        {
            if (entity.mainTarget == entity.home)
            {
                entity.target = entity.home;
                entity.returningHome = true;
                return;
            }

            // Arrived at harvestable → take items
            var inv = entity.GetComponent<Inventory>();
            var targetInv = entity.mainTarget.GetComponent<EntityInventory>();
            Debug.Log("Harvest resource");
            targetInv.GiveItemToOther(harvestItem, giveAmount, inv);

            if (targetInv.IsEmpty(harvestItem))
                world.DestroyFoundHarvestable(entity.mainTarget.position);

            // Return home next
            entity.target = entity.home;
            entity.returningHome = true;
        }
    }


}