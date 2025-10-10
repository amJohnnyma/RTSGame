using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EntityMovementManager : MonoBehaviour
{
    public List<EntityRuntime> entities = new();

    // make default tasks if needed
    private TaskCreator taskCreator;

    private bool updatedToZeroFlag = false;


    void Start()
    {
        //temporary
        GameObject[] e = GameObject.FindGameObjectsWithTag("EntityMoveable");
        foreach (var i in e)
        {
            entities.Add(i.GetComponent<EntityRuntime>());

        }

        taskCreator = GetComponent<TaskCreator>();
    }

    public void EntityPausedMovement()
    {
        if (updatedToZeroFlag) return;
        foreach (var i in entities)
        {
            i.rb.velocity = Vector3.zero;
        }
        updatedToZeroFlag = true;
    }



    public void EntityMovementUpdates(World world)
    {
        updatedToZeroFlag = false;
        world.RefreshHarvestableCache();

        Vector3[] positions = new Vector3[entities.Count];
        Vector3[] targetPositions = new Vector3[entities.Count];
        ITask[] task = new ITask[entities.Count];

        // parallel data
        CreateEntityLists(positions, targetPositions, task);

        //Assign tasks to entities without

        // compute parallel movement
        ComputeParallelMovement(entities, positions, targetPositions, task);

        //Main thread for api calls
        MainThreadCall(world, task);



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

    void MainThreadCall(World world, ITask[] task)
    {
        int count = 0;
        // --- Phase 2: Main thread - raycast to terrain and move Rigidbody ---
        foreach (var entity in entities)
        {
            // can we even move this entity
            if (entity.rb == null)
            {
                count++;
                continue;
            }

            AssignEntityTasks(entity, world);
            Debug.Log("Doing task: " + entity.currentTask.GetType());
            Debug.Log("Global available: " + taskCreator.taskList.GetTaskCount());


            if (EntityHasBadTarget(entity))
            {
                count++;
                continue;

            }

            ResetEntityPendingtarget(entity, world);


            if (EntityReachedTarget(entity, task, count))
            {
                count++;
                continue;
            }


            EntityPhysics(entity);


            count++;

        }

    }

    void CreateEntityLists(Vector3[] positions, Vector3[] targetPositions, ITask[] task)
    {
        for (int i = 0; i < entities.Count; i++)
        {
            positions[i] = entities[i].transform.position;      // main thread copy
            targetPositions[i] = entities[i].target == null ? entities[i].home.transform.position : entities[i].target.transform.position;   // main thread copy
            // default task to be assigned
            task[i] = taskCreator.CreateTask(entities[i]); // and assign to entity if needed
        }

    }

    void ComputeParallelMovement(List<EntityRuntime> entities, Vector3[] positions, Vector3[] targetPositions, ITask[] task)
    {
        Parallel.For(0, entities.Count, i =>
        {
            var entity = entities[i];
            if (targetPositions[i] == null) targetPositions[i] = positions[i];

            entity.behaviorHandler.ComputeMove(entity, positions[i], targetPositions[i], task[i]);



        });

    }

    void ComputeNewEntityVelocity(EntityRuntime entity, Vector3 tangentDir)
    {
        Vector3 desiredVel = tangentDir * entity.moveSpeed;
        entity.rb.velocity = Vector3.Lerp(entity.rb.velocity, desiredVel, Time.fixedDeltaTime * entity.turnSpeed);

        float maxSpeed = entity.moveSpeed * 1.5f; // safety buffer
        if (entity.rb.velocity.magnitude > maxSpeed)
            entity.rb.velocity = entity.rb.velocity.normalized * maxSpeed;

    }

    void MoveEntity(EntityRuntime entity, Vector3 groundPoint, Vector3 groundNormal)
    {
        // Stick to terrain surface
        entity.rb.MovePosition(groundPoint + groundNormal * entity.surfaceOffset);

        // Align rotation with terrain
        Quaternion targetRot = Quaternion.FromToRotation(entity.transform.up, groundNormal) * entity.transform.rotation;
        entity.rb.MoveRotation(Quaternion.Slerp(entity.rb.rotation, targetRot, Time.fixedDeltaTime * entity.turnSpeed));
        entity.rb.AddForce(-entity.transform.up * 20f, ForceMode.Acceleration);

    }

// perform physics for the rigidbody
    void EntityPhysics(EntityRuntime entity)
    {

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
            ComputeNewEntityVelocity(entity, tangentDir);

            MoveEntity(entity, groundPoint, groundNormal);



        }
        else
        {
            entity.rb.velocity = Vector3.zero;
        }

    }

// target and main target null -> stop moving and set target to home
    bool EntityHasBadTarget(EntityRuntime entity)
    {
        if (entity.target == null)
        {
            entity.target = entity.home;
            entity.rb.velocity = Vector3.zero;
            return true;
        }
        if (entity.mainTarget == null)
        {
            entity.mainTarget = entity.home;
            entity.rb.velocity = Vector3.zero;
            return true;
        }

        return false;

    }

// set the target to the new point
    void ResetEntityPendingtarget(EntityRuntime entity, World world)
    {
        if (entity.pendingTargetPos != Vector3.positiveInfinity)
        {
            if (world.placedEntities.TryGetValue(entity.pendingTargetPos, out var go))
            {
                entity.target = go.transform;
            }
            entity.pendingTargetPos = Vector3.positiveInfinity; // reset
        }

    }

// are they close enough to the target?
    bool EntityReachedTarget(EntityRuntime entity, ITask[] task, int count)
    {
        if ((entity.target.position - entity.transform.position).sqrMagnitude < entity.stopFollowDist * entity.stopFollowDist)
        {
            entity.rb.velocity = Vector3.zero;
            // Generic target toggle (mainTarget/home switching)
            entity.SetTargetToggle();

            // Behavior-specific effects
            entity.behaviorHandler.OnTargetReached(entity, entity.currentTask);
            return true;
        }

        return false;

    }


    // this needs to be cleaner
/*
    if no task get a task return;

    if current not complete, finish it return;

    if idle or home -> can take a global task return;


    // Task system should get a rework
    // the entity shouldnt think for itself, the task must dictate what it does -> Check entity movement stuff aswell

*/
    bool AssignEntityTasks(EntityRuntime entity, World world)
{
    ITask current = entity.currentTask;

    // --- Clean up finished tasks ---
    if (current != null && current.IsComplete)
    {
        entity.currentTask = null;
        current = null;
    }

    bool hasHarvestables = world.IsUnfoundHarvestables();
    bool hasGlobalTasks = taskCreator.taskList.GetTaskCount() > 0;
    bool isAtHome = entity.IsAtHome();

    
    if (hasGlobalTasks)
    {
        Debug.Log("Are global tasks");
        if (current == null || current.Type == TaskType.Idle || current.Type == TaskType.Home)
        {
            // Take a task from the global list
            taskCreator.CreateTask(entity);
            return true;
        }
        return false; // already working on a non-idle task
    }

    // --- CASE 2: Harvestables exist but no global tasks ---
    if (hasHarvestables)
    {
        if (current == null || current.Type == TaskType.Idle || current.Type == TaskType.Home)
        {
            // Assign default harvest/scout task
            taskCreator.CreateTask(entity);
            return true;
        }
        return false; // already doing something
    }

    // --- CASE 3: No global tasks and no harvestables ---
    if (!hasHarvestables && !hasGlobalTasks)
    {
        if (isAtHome)
        {
            if (current == null || current.Type != TaskType.Idle)
            {
                taskCreator.CreateTaskForEntity(TaskType.Idle, "IdleHome", entity);
                return true;
            }
            return false; // already idling
        }
        else
        {
            if (current == null || current.Type != TaskType.Home)
            {
                if (entity.home != null)
                {
                    taskCreator.CreateTaskForEntity(TaskType.Home, "ReturnHome", entity);
                    return true;
                }
                Debug.LogWarning($"Entity {entity.name} has no home assigned, cannot return home.");
                return false;
            }
            return false; // already returning home
        }
    }

    return false;
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