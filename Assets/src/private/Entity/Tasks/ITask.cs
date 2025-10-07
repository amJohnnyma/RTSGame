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



public interface ITask
{
    TaskType Type { get; }
    Vector3 TargetPos { get; }
    Transform HomePos { get; set; }
    int Priority { get; }
    bool IsComplete { get; }
    bool HasStarted { get; }

    bool SetHasStarted { set; }
    bool SetIsComplete { set; }

    public Transform SetHomePos { set => HomePos = value; }

    void UpdateTask(EntityRuntime entity, Vector3 entityPos, Vector3 targetPos, World world, float visionRadius);

    void OnTargetReached(EntityRuntime entity, World world);

    public string GetTaskDetails(EntityRuntime e);


    
}

// find resources
public class ScoutResources : ITask
{
    public TaskType Type => TaskType.Scout;
    public Vector3 TargetPos { get; private set; }
    public int Priority { get; private set; }
    public bool IsComplete { get; private set; }
    public bool HasStarted { get; private set; }


    public bool SetHasStarted { set => HasStarted = value; }
    public bool SetIsComplete { set => IsComplete = value; }

    public Transform HomePos {get;  set;}

    public ScoutResources(Vector3 target, int priority = 1)
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
            if(HomePos != null)
                entity.home = HomePos;
            entity.target = entity.home;
            entity.returningHome = true;
        }



    }

    public string GetTaskDetails(EntityRuntime e)
    {
        string typeString = Type.ToString() ?? "none";
        string entityBehaviourString = e.behaviour.ToString() ?? "none";
        string text = "Task: " + typeString + "\tEntityBehaviour: " + entityBehaviourString + "\tSpecific: ScoutResources";
        return text;
    }

}

// just harvest whatever you want
public class HarvestRandom : ITask
{
    public TaskType Type => TaskType.Harvest;
    public Vector3 TargetPos { get; private set; }
    public int Priority { get; private set; }
    public bool IsComplete { get; private set; }
    public bool HasStarted { get; private set; }


    public bool SetHasStarted { set => HasStarted = value; }
    public bool SetIsComplete { set => IsComplete = value; }

    public Transform HomePos {get; set;}

    public HarvestRandom(Vector3 target, int priority = 1)
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
            if(HomePos != null)
                entity.home = HomePos;
            entity.target = entity.home;
            entity.returningHome = true;
        }
    }

    public string GetTaskDetails(EntityRuntime e)
    {
        string typeString = Type.ToString() ?? "none";

        string entityBehaviourString = e.behaviour.ToString() ?? "none";
        string text = "Task: " + typeString + "\tEntityBehaviour: " + entityBehaviourString + "\tSpecific: HarvestRandom";
        return text;
    }

}

/*
// Harvest the flower only once
entity.taskList.AddTask(new HarvestSpecific(flower, cycles: 1));

// Harvest the flower 5 times (5 full trips)
entity.taskList.AddTask(new HarvestSpecific(flower, cycles: 5));

// Collect exactly 30 resources (may take multiple trips)
entity.taskList.AddTask(new HarvestSpecific(flower, targetAmount: 30));

// Harvest until resource is empty (infinite)
entity.taskList.AddTask(new HarvestSpecific(flower, infinite: true));
*/
public class HarvestSpecific : ITask
{
    public TaskType Type => TaskType.Harvest;
    public Vector3 TargetPos { get; private set; }
    public int Priority { get; private set; }
    public bool IsComplete { get; private set; }
    public bool HasStarted { get; private set; }

    public bool SetHasStarted { set => HasStarted = value; }
    public bool SetIsComplete { set => IsComplete = value; }

    public Transform HomePos {get; set;}

    private GameObject specificTarget; 
    private int remainingCycles;       // how many full trips (harvest + return)
    private int collectedSoFar;        // track total resources taken
    private int targetAmount;          // stop once this amount is collected
    private bool infinite;             // keep going until resource is empty

    public HarvestSpecific(GameObject target, int cycles = 1, int targetAmount = -1, bool infinite = false, int priority = 1)
    {
        specificTarget = target;
        TargetPos = target.transform.position;
        Priority = priority;
        IsComplete = false;
        HasStarted = false;

        this.remainingCycles = cycles;
        this.targetAmount = targetAmount;
        this.infinite = infinite;
        this.collectedSoFar = 0;
    }

    public void UpdateTask(EntityRuntime entity, Vector3 entityPos, Vector3 targetPos, World world, float visionRadius)
    {
        if (!entity.returningHome && specificTarget != null)
        {
            entity.pendingTargetPos = specificTarget.transform.position;
            TargetPos = entity.pendingTargetPos;
        }

        var nearbyPoints = EntityMovementManager.GetNearbyPoints(entityPos, entity.radius, entity.checkPoints);
        entity.lastNearbyPoints = nearbyPoints;

        float bestScore = float.MinValue;
        int bestIdx = 0;
        for (int j = 0; j < nearbyPoints.Length; j++)
        {
            float score = 1f / ((nearbyPoints[j] - TargetPos).sqrMagnitude + 0.001f);
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
        }

        if (entity.returningHome)
        {
            // Deliver to home
            var homeInv = entity.home.GetComponent<Inventory>();
            int carried = takeAmount;
            entity.GetComponent<Inventory>().GiveItemToOther(harvestItem, takeAmount, homeInv);

            // Count delivered
            collectedSoFar += carried;

            // Decide whether to stop
            if (!infinite)
            {
                if (targetAmount > 0 && collectedSoFar >= targetAmount)
                {
                    IsComplete = true;
                    return;
                }
                if (remainingCycles > 0)
                {
                    remainingCycles--;
                    if (remainingCycles == 0)
                    {
                        IsComplete = true;
                        return;
                    }
                }
            }

            // If target still exists, go again
            if (specificTarget != null)
            {
                entity.mainTarget = specificTarget.transform;
                entity.target = entity.mainTarget;
                entity.returningHome = false;
            }
            else
            {
                IsComplete = true;
            }
        }
        else
        {
            // Arrived at harvestable
            if (specificTarget == null)
            {
                IsComplete = true;
                return;
            }

            var inv = entity.GetComponent<Inventory>();
            var targetInv = specificTarget.GetComponent<EntityInventory>();
            targetInv.GiveItemToOther(harvestItem, giveAmount, inv);

            if (targetInv.IsEmpty(harvestItem))
            {
                world.DestroyFoundHarvestable(specificTarget.transform.position);
                specificTarget = null;
            }

            // Return home next
            if(HomePos != null)
                entity.home = HomePos;
            entity.target = entity.home;
            entity.returningHome = true;
        }
    }
    public string GetTaskDetails(EntityRuntime e)
    {
        string typeString = Type.ToString() ?? "none";
        string entityBehaviourString = e.behaviour.ToString() ?? "none";
        string text = "Task: " + typeString + "\tEntityBehaviour: " + entityBehaviourString + "\tSpecific: HarvestSpecific";
        return text;
    }
}


public class ReturnHome : ITask
{
    public TaskType Type => TaskType.Home;
    public Vector3 TargetPos { get; private set; }
    public int Priority { get; private set; }
    public bool IsComplete { get; private set; }
    public bool HasStarted { get; private set; }


    public bool SetHasStarted { set => HasStarted = value; }
    public bool SetIsComplete { set => IsComplete = value; }

    public Transform HomePos { get; set; }

    public ReturnHome(Vector3 target, int priority = 1)
    {
        TargetPos = target;
        Priority = priority;
        IsComplete = false;
        HasStarted = false;
    }

    public void UpdateTask(EntityRuntime entity, Vector3 entityPos, Vector3 targetPos, World world, float visionRadius)
    {

        entity.returningHome = true;

        // this will be set to home on creation
        //  entity.pendingTargetPos = TargetPos;

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
            if(HomePos != null)
                entity.home = HomePos;
            // Arrived home → deliver items
            var homeInv = entity.home.GetComponent<Inventory>();
            entity.GetComponent<Inventory>().GiveItemToOther(harvestItem, takeAmount, homeInv);

            if (!world.IsUnfoundHarvestables())
            {
                // No work left → idle
                entity.taskList.ClearTasks();
                entity.taskList.AddTask(new IdleTask(9)); // low prio
                entity.rb.velocity = Vector3.zero;
                return;
            }

            // Otherwise pick next harvestable
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
            // (same as before, harvest logic…)
            if (entity.mainTarget == entity.home)
            {
                entity.target = entity.home;
                entity.returningHome = true;
                return;
            }

            var inv = entity.GetComponent<Inventory>();
            var targetInv = entity.mainTarget.GetComponent<EntityInventory>();
            targetInv.GiveItemToOther(harvestItem, giveAmount, inv);

            if (targetInv.IsEmpty(harvestItem))
                world.DestroyFoundHarvestable(entity.mainTarget.position);

            if(HomePos != null)
                entity.home = HomePos;
            entity.target = entity.home;
            entity.returningHome = true;
        }
    }

    public string GetTaskDetails(EntityRuntime e)
    {
        string typeString = Type.ToString() ?? "none";
        string entityBehaviourString = e.behaviour.ToString() ?? "none";
        string text = "Task: " + typeString + "\tEntityBehaviour: " + entityBehaviourString + "\tSpecific: ReturnHome";
        return text;
    }

}

public class IdleTask : ITask
{
    public TaskType Type => TaskType.Idle;
    public Vector3 TargetPos => Vector3.zero;
    public int Priority  {    get; private set;}
    public bool IsComplete { get; private set; }
    public bool HasStarted { get; private set; }
    public bool IsIdling { get; private set; }

    public bool SetHasStarted { set => HasStarted = value; }
    public bool SetIsComplete { set => IsComplete = value; }

    public Transform HomePos { get; set; }

    public IdleTask(int priority)
    {
        Priority = priority;
        Debug.Log("Created IDLE");
    }
    

    public void UpdateTask(EntityRuntime entity, Vector3 entityPos, Vector3 targetPos, World world, float visionRadius)
    {
        // Do nothing
        //   entity.rb.velocity = Vector3.zero;
        entity.home = HomePos;
    }

    public void OnTargetReached(EntityRuntime entity, World world)
    {
        // Stay idle
        IsComplete = false;
        IsIdling = true; // we are at the target but dont complete the task
    }
    public string GetTaskDetails(EntityRuntime e)
    {
        string typeString = Type.ToString() ?? "none";
        string entityBehaviourString = e.behaviour.ToString() ?? "none";
        string text = "Task: " + typeString + "\tEntityBehaviour: " + entityBehaviourString + "\tSpecific: IdleTask";
        return text;
    }
}

public class GoToTask : ITask
{
    public TaskType Type => TaskType.GoTo;
    public Vector3 TargetPos { get; private set; }
    public int Priority { get; private set; }
    public bool IsComplete { get; private set; }
    public bool HasStarted { get; private set; }

    public bool SetHasStarted { set => HasStarted = value; }
    public bool SetIsComplete { set => IsComplete = value; }
    public Transform HomePos { get; set; }

    public GoToTask(Vector3 targetPos, int priority = 5)
    {
        TargetPos = targetPos;
        Priority = priority;
        IsComplete = false;
        HasStarted = false;
    }

    public void UpdateTask(EntityRuntime entity, Vector3 entityPos, Vector3 targetPos, World world, float visionRadius)
    {
        // --- Compute movement (same pattern as ReturnHome) ---
        var nearbyPoints = EntityMovementManager.GetNearbyPoints(entityPos, entity.radius, entity.checkPoints);
        entity.lastNearbyPoints = nearbyPoints;

        float bestScore = float.MinValue;
        int bestIdx = 0;

        for (int j = 0; j < nearbyPoints.Length; j++)
        {
            float score = 1f / ((nearbyPoints[j] - TargetPos).sqrMagnitude + 0.001f);
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
        IsComplete = true;
        entity.rb.velocity = Vector3.zero;

        // Optionally set entity to idle
        entity.taskList.ClearTasks();
        entity.taskList.AddTask(new IdleTask(9));
    }

    public string GetTaskDetails(EntityRuntime e)
    {
        return $"Task: {Type}\tEntityBehaviour: {e.behaviour}\tTarget: {TargetPos}";
    }
}
