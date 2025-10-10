using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EntityMovementManager : MonoBehaviour
{
    public List<EntityRuntime> entities = new();

    private TaskCreator taskCreator;

    void Start()
    {
        GameObject[] e = GameObject.FindGameObjectsWithTag("EntityMoveable");
        foreach (var i in e)
            entities.Add(i.GetComponent<EntityRuntime>());

        taskCreator = GetComponent<TaskCreator>();
    }

    public void EntityPausedMovement()
    {
        
    }

    public void EntityMovementUpdates(World world)
    {
        world.RefreshHarvestableCache();

        Vector3[] positions = new Vector3[entities.Count];

        for (int i = 0; i < entities.Count; i++)
        {
            positions[i] = entities[i].transform.position;
            AssignEntityTasks(entities[i]);

        }

        // --- Phase 1: Parallel compute ---
        Parallel.For(0, entities.Count, i =>
        {
            var entity = entities[i];
            if (entity.currentTask != null)
            {
                entity.currentTask.UpdateTask(positions[i]);
            }
        });

        // --- Phase 2: Main thread physics ---
        foreach (var entity in entities)
        {
            if (entity.currentTask == null)
                continue;

            HandleMovement(entity);
        }
    }

    void HandleMovement(EntityRuntime entity)
    {
        Vector3 downDir = -entity.transform.up;
        if (Physics.Raycast(entity.transform.position + downDir * 0.1f, downDir,
                            out RaycastHit hit, entity.raycastDownDist, 1 << entity.worldCollider.gameObject.layer))
        {
            Vector3 groundPoint = hit.point;
            Vector3 groundNormal = hit.normal;

            Vector3 moveVec = entity.lastNearbyPoints[entity.chosenScore] - entity.transform.position;
            Vector3 tangentDir = Vector3.ProjectOnPlane(moveVec, groundNormal).normalized;

            if (tangentDir.sqrMagnitude < 0.0001f)
                tangentDir = Vector3.Cross(hit.normal, Vector3.forward).normalized;

            Vector3 desiredVel = tangentDir * entity.moveSpeed;
            entity.rb.velocity = Vector3.Lerp(entity.rb.velocity, desiredVel, Time.fixedDeltaTime * entity.turnSpeed);

            entity.rb.MovePosition(groundPoint + groundNormal * entity.surfaceOffset);

            Quaternion targetRot = Quaternion.FromToRotation(entity.transform.up, groundNormal) * entity.transform.rotation;
            entity.rb.MoveRotation(Quaternion.Slerp(entity.rb.rotation, targetRot, Time.fixedDeltaTime * entity.turnSpeed));

            // --- Target reached check ---
            if ((entity.currentTask.GetTargetPos() - entity.transform.position).sqrMagnitude < entity.stopFollowDist * entity.stopFollowDist)
            {
                entity.currentTask.OnTargetReached();
                entity.currentTask = null;
                entity.rb.velocity = Vector3.zero;
            }
        }
    }

    void AssignEntityTasks(EntityRuntime entity)
    {
        if (entity.currentTask != null && !entity.currentTask.IsTaskComplete())
            return;

        if(entity.currentTask.IsTaskComplete())
        {
            // check what type of task

            // make it idle, or go home, or whatever
        }

        // these are user created tasks
        var newTask = taskCreator.TryAssignGlobalTask(entity);

        // if newTask is null then ask the 'brain' -> The default task assignment to be implemented

        if (newTask != null)
        {
            entity.currentTask = newTask;
            Debug.Log($"Assigned GoTo task to {entity.name}: {newTask.GetTaskDetails()}");
        }
    }

    // Reuse your Fibonacci sphere sampling
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