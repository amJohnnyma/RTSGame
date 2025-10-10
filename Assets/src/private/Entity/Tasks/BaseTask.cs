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

public abstract class BaseTask
{
    public EntityRuntime entity { get; private set; }
    protected Vector3 targetPos; // Current position to move toward
    protected bool isCompleted = false;
    protected readonly int repeat;

    protected BaseTask(EntityRuntime entity, int repeat = 0)
    {
        this.entity = entity;
        if (entity != null)
            entity.currentTask = this;

        this.repeat = repeat;
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


public class GoToTask : BaseTask
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


public class ScoutTask : BaseTask
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
        // do all repeats first
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


public class HarvestTask : BaseTask
{
    private readonly World world;
    private Vector3 homePos;
    private Vector3 currentHarvestable;
    private bool returningHome = false;

    // what am i harvesting? 
    // How much is needed? (Priority over repeats)
    // Where must i deposit the resources?
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
            //do all repeats first
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


