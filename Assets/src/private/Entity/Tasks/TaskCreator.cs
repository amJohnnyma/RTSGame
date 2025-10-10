using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TaskCreator : MonoBehaviour
{
    private readonly List<BaseTask> globalTasks = new();

    public World world; // Reference to the world

    public void Start()
    {
        world = GetComponent<World>();
    }


    // --- Create a new global task ---
    public void CreateTask(TaskType type, string specialization, Transform position, int priority = 1, float visionRadius = 5f)
    {
        for (int i = 0; i < priority; i++)
        {
            BaseTask task = type switch
            {
                TaskType.GoTo => new GoToTask(null, position),
                TaskType.Scout => new ScoutTask(null, world, visionRadius),
                TaskType.Harvest => new HarvestTask(null, world),
                _ => null
            };

            if (task != null)
            {
                globalTasks.Add(task);
                Debug.Log($"[TaskCreator] Created {type} task at {(position != null ? position.name : "null")}");
            }
        }
    }

    // --- Assign first available task to an entity ---
    public BaseTask TryAssignGlobalTask(EntityRuntime entity)
    {
        if (globalTasks.Count == 0)
            return null;

        // Pick a task matching the entity behaviour first
        BaseTask task = PickTaskForEntity(entity);

        if (task == null) return null;

        globalTasks.Remove(task); // Remove immediately once claimed
        task.AssignEntity(entity);
        entity.currentTask = task;

        Debug.Log($"[TaskCreator] Assigned {task.GetTaskDetails()} to {entity.name}");
        return task;
    }

    // --- Pick task based on entity behaviour ---
    private BaseTask PickTaskForEntity(EntityRuntime entity)
    {
        TaskType preferredType = entity.behaviour switch
        {
            EntityBehaviour.SCOUT => TaskType.Scout,
            EntityBehaviour.HARVEST => TaskType.Harvest,
            _ => TaskType.GoTo
        };

        // Find task that matches behaviour or fallback to GoTo
        return globalTasks.FirstOrDefault(t =>
        {
            TaskType taskType = GetTaskType(t);
            return taskType == preferredType || taskType == TaskType.GoTo;
        });
    }

    // --- Helper to determine type of BaseTask ---
    private TaskType GetTaskType(BaseTask task)
    {
        return task switch
        {
            GoToTask => TaskType.GoTo,
            ScoutTask => TaskType.Scout,
            HarvestTask => TaskType.Harvest,
            _ => TaskType.Idle
        };
    }

    // --- Debug / Info ---
    public int GetTaskCount() => globalTasks.Count;
}
