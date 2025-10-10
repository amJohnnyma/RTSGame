using UnityEngine;

public enum TaskType
{
    Scout,
    Harvest,
    Attack,
    Idle,
    Home,
    GoTo
}

public abstract class ITask
{
    public EntityRuntime entity { get; private set; }
    protected Vector3 targetPos; // Current position to move toward
    protected bool isCompleted = false;

    protected ITask(EntityRuntime entity)
    {
        this.entity = entity;
        if (entity != null)
            entity.currentTask = this;
    }

    public virtual void AssignEntity(EntityRuntime newEntity)
    {
        this.entity = newEntity;
        entity.currentTask = this;
    }


    public virtual void UpdateTask(Vector3 entityPos)
    {
        var nearbyPoints = EntityMovementManager.GetNearbyPoints(entityPos, entity.radius, entity.checkPoints);
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
        entity.pendingTargetPos = targetPos;
    }

    public bool IsTaskComplete() => isCompleted;

    public abstract void OnTargetReached();
    public abstract string GetTaskDetails();
    public Vector3 GetTargetPos() => targetPos;
}


public class GoToTask : ITask
{
    private readonly Transform target;

    public GoToTask(EntityRuntime entity, Transform target)
        : base(entity)
    {
        this.target = target;
        targetPos = target != null ? target.position : Vector3.zero;
    }

    public override void OnTargetReached()
    {
        Debug.Log($"{entity.name} reached {target?.name ?? "None"}");
        entity.rb.velocity = Vector3.zero;
        isCompleted = true;
    }

    public override string GetTaskDetails() => $"GoTo Task (Target: {target?.name ?? "None"})";
}


public class ScoutTask : ITask
{
    private readonly World world;
    private readonly float visionRadius;
    private Vector3 homePos;
    private Vector3 currentTarget;

    public ScoutTask(EntityRuntime entity, World world, float visionRadius)
        : base(entity)
    {
        this.world = world;
        this.visionRadius = visionRadius;
        //homePos = entity.home.position;
        PickNewTarget();
    }

    public override void AssignEntity(EntityRuntime newEntity)
    {
        base.AssignEntity(newEntity);
        homePos = newEntity.home.position;
        PickNewTarget();
    }

    public override void UpdateTask(Vector3 entityPos)
    {
        // If no target or target destroyed, pick new
        if (currentTarget == null)
            PickNewTarget();

        targetPos = currentTarget != null ? currentTarget : homePos;

        base.UpdateTask(entityPos);
    }

    public override void OnTargetReached()
    {
        if (currentTarget != null)
            world.AddFoundHarvestable(currentTarget);

        // Return home next
        targetPos = homePos;
        entity.returningHome = true;
        isCompleted = true;
    }

    public override string GetTaskDetails()
    {
        string targetName = currentTarget != null ? currentTarget.ToString() : "None";
        return $"Scout Task (Target: {targetName})";
    }

    private void PickNewTarget()
    {
        GameObject go = world.GetRandomPlacedHarvestable();
        currentTarget = go != null ? go.transform.position : Vector3.zero;
    }
}


public class HarvestTask : ITask
{
    private readonly World world;
    private Vector3 homePos;
    private Vector3 currentHarvestable;
    private bool returningHome = false;

    public HarvestTask(EntityRuntime entity, World world)
        : base(entity)
    {
        this.world = world;
      //  homePos = entity.home.position;
        PickNextHarvestable();
    }

    public override void AssignEntity(EntityRuntime newEntity)
    {
        base.AssignEntity(newEntity);
        homePos = newEntity.home.position;
        PickNextHarvestable();
    }

    public override void UpdateTask(Vector3 entityPos)
    {
        // Ensure we have a target
        if (!returningHome && currentHarvestable == null)
            PickNextHarvestable();

        targetPos = returningHome ? homePos : currentHarvestable;

        base.UpdateTask(entityPos);
    }

    public override void OnTargetReached()
    {
        if (!returningHome)
        {
            if (currentHarvestable != null)
                world.DestroyFoundHarvestable(currentHarvestable);

            returningHome = true;
            targetPos = homePos;
            isCompleted = false;
        }
        else
        {
            returningHome = false;
            PickNextHarvestable();
            isCompleted = true;
        }
    }

    public override string GetTaskDetails()
    {
        string targetName = currentHarvestable != null ? currentHarvestable.ToString() : "None";
        return $"Harvest Task (Target: {targetName}, ReturningHome: {returningHome})";
    }

    private void PickNextHarvestable()
    {
        GameObject go = world.GetRandomFoundHarvestable() ?? world.GetRandomPlacedHarvestable();
        currentHarvestable = go != null ? go.transform.position : Vector3.zero;
    }
}



/*

using UnityEngine;

public enum TaskType
{
    Scout,
    Harvest,
    Attack,
    Idle,
    Home,
    GoTo
}

public abstract class ITask
{
    public EntityRuntime entity { get; private set; }
    public Transform mainTarget;
    public Vector3 targetVector = Vector3.zero; 

    protected bool isCompleted = false;


    protected ITask(EntityRuntime entity, Transform mainTarget)
    {
        this.entity = entity;
        this.mainTarget = mainTarget;
        targetVector = mainTarget != null ? mainTarget.position : Vector3.zero;

        if (entity != null)
        {
            entity.currentTask = this;
        }


    }

    public virtual void AssignEntity(EntityRuntime newEntity)
    {
        this.entity = newEntity;

        entity.currentTask = this;
    }

    // Purely numerical logic — safe for parallel threads
    public virtual void UpdateTask(Vector3 entityPos)
    {
        var nearbyPoints = EntityMovementManager.GetNearbyPoints(
            entityPos,
            entity.radius,
            entity.checkPoints
        );

        entity.lastNearbyPoints = nearbyPoints;

        float bestScore = float.MinValue;
        int bestIdx = 0;

        for (int j = 0; j < nearbyPoints.Length; j++)
        {
            float score = 1f / ((nearbyPoints[j] - targetVector).sqrMagnitude + 0.001f);
            if (score > bestScore)
            {
                bestScore = score;
                bestIdx = j;
            }
        }

        entity.chosenScore = bestIdx;
    }

    virtual public bool IsTaskComplete()
    {
        return isCompleted;
    }

    // Called when we reach the target (main thread)
    public abstract void OnTargetReached();

    // Useful for UI/debug/logging
    public abstract string GetTaskDetails();

    public Vector3 GetTargetPos()
    {
        return targetVector;
    }
}

public class GoToTask : ITask
{
    public GoToTask(EntityRuntime entity, Transform target)
        : base(entity, target) { }

    public override void OnTargetReached()
    {
        Debug.Log($"{entity.name} reached {mainTarget.name}");
        entity.rb.velocity = Vector3.zero;
    }

    public override string GetTaskDetails()
    {
        string target = mainTarget == null ? "None" : mainTarget.name;
        return $"GoTo Task (Target: {target})";

    } 
}


public class ScoutTask : ITask
{
    private readonly World world;
    private readonly float visionRadius;
    private Vector3 homePos;

    public ScoutTask(EntityRuntime entity, Transform target, World world, float visionRadius)
        : base(entity, target)
    {
        this.world = world;
        this.visionRadius = visionRadius;

        if (entity != null)
            homePos = entity.home.position;
    }

    public override void AssignEntity(EntityRuntime newEntity)
    {
        base.AssignEntity(newEntity);
        if (newEntity != null)
            homePos = newEntity.home.position;
    }

    public override void UpdateTask(Vector3 entityPos)
    {
        // --- Step 0: Pick a target if none
        if (mainTarget == null)
        {
            GameObject go = world.GetRandomPlacedHarvestable(); // just pick a harvestable
            if (go != null)
                mainTarget = go.transform;
        }

        Vector3 currentTargetPos = mainTarget?.position ?? Vector3.positiveInfinity;

        // --- Step 1: Parallel-safe movement scoring ---
        var nearbyPoints = EntityMovementManager.GetNearbyPoints(entityPos, entity.radius, entity.checkPoints);
        entity.lastNearbyPoints = nearbyPoints;

        float bestScore = float.MinValue;
        int bestIdx = 0;

        for (int j = 0; j < nearbyPoints.Length; j++)
        {
            float score = 1f / ((nearbyPoints[j] - currentTargetPos).sqrMagnitude + 0.001f);
            if (score > bestScore)
            {
                bestScore = score;
                bestIdx = j;
            }
        }

        entity.chosenScore = bestIdx;
        entity.pendingTargetPos = currentTargetPos;
    }

    public override void OnTargetReached()
    {
        if (entity.returningHome) isCompleted = true;
        // Arrived at harvestable → mark as found
        if (!entity.returningHome)
        {
            if (mainTarget != null)
                world.AddFoundHarvestable(mainTarget.position, mainTarget.gameObject);
        }

        // Return home next
        targetVector = entity.homePos;
        entity.returningHome = true;

        }

    public override string GetTaskDetails()
    {
        string targetName = mainTarget != null ? mainTarget.name : "None";
        return $"Scout Task (Target: {targetName})";
    }
}


public class HarvestTask : ITask
{
    private readonly World world;
    private Vector3 homePos;

    public HarvestTask(EntityRuntime entity, Transform target, World world)
        : base(entity, target)
    {
        this.world = world;
        if (entity != null)
            homePos = entity.home.position;
    }

    public override void AssignEntity(EntityRuntime newEntity)
    {
        base.AssignEntity(newEntity);
        if (newEntity != null)
            homePos = newEntity.home.position;

        if (mainTarget == null)
            mainTarget = PickNextHarvestable();

        if (mainTarget != null)
            targetVector = mainTarget.position;
        else
            targetVector = entity.homePos;
    }

    public override void UpdateTask(Vector3 entityPos)
    {
        if (mainTarget == null)
            mainTarget = PickNextHarvestable();

        Vector3 currentTargetPos = mainTarget != null
            ? targetVector
            : entity.homePos;

        // Parallel-safe movement scoring
        var nearbyPoints = EntityMovementManager.GetNearbyPoints(entityPos, entity.radius, entity.checkPoints);
        entity.lastNearbyPoints = nearbyPoints;

        float bestScore = float.MinValue;
        int bestIdx = 0;

        for (int j = 0; j < nearbyPoints.Length; j++)
        {
            float score = 1f / ((nearbyPoints[j] - currentTargetPos).sqrMagnitude + 0.001f);
            if (score > bestScore)
            {
                bestScore = score;
                bestIdx = j;
            }
        }

        entity.chosenScore = bestIdx;
        entity.pendingTargetPos = currentTargetPos;
    }

    public override void OnTargetReached()
    {
        if (!entity.returningHome)
        {
            // Harvest the resource
            if (mainTarget != null)
                world.DestroyFoundHarvestable(mainTarget.position);

            // Return home next
            targetVector = entity.homePos;
            entity.returningHome = true;

            isCompleted = true;
        }
        else
        {
            // Arrived home → pick next harvestable
            mainTarget = PickNextHarvestable();

            if (mainTarget != null)
            {
                targetVector = mainTarget.position;
                entity.returningHome = false;
            }
            else
            {
                // No harvestables → stay at home
                targetVector = entity.homePos;
                entity.returningHome = true;
            }

            isCompleted = false; // ready for next cycle
        }
    }

    public override string GetTaskDetails()
    {
        string targetName = mainTarget != null ? mainTarget.name : "None";
        return $"Harvest Task (Target: {targetName})";
    }

    private Transform PickNextHarvestable()
    {
        // 1. Prefer harvestables found by scouts
        GameObject go = world.GetRandomFoundHarvestable();

        // 2. Fallback to any available harvestable in the world
        if (go == null)
            go = world.GetRandomPlacedHarvestable();

        return go != null ? go.transform : null;
    }
}
*/