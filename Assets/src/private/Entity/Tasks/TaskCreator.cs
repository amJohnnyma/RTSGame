using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

// the task itself needs to specify the target, and return point
public class TaskCreator : MonoBehaviour
{

    [SerializeField] private World world; // reference world -> Which planet are we one

    public TaskList taskList = new TaskList();

    // create a task for this entity
    public void CreateTaskForEntity(TaskType type, string specialization, EntityRuntime entity)
    {
        if (type == TaskType.Home)
        {
            var current = entity.taskList.GetCurrentTask();
            if (current == null || current.Type != TaskType.Home)
            {
                ITask task = new ReturnHome(entity.home.transform.position, 0);
                entity.target = entity.home;
                entity.mainTarget = entity.home;
                entity.taskList.ClearTasks();
                entity.taskList.AddTask(task);
            }
        }
        else if (type == TaskType.Idle)
        {
            var current = entity.taskList.GetCurrentTask();
            if (current == null || current.Type != TaskType.Idle)
            {
                ITask task = new IdleTask(9);
                entity.taskList.ClearTasks();
                entity.taskList.AddTask(task);
            }
        }
        else if (type == TaskType.Scout)
        {
        }
        else if (type == TaskType.Harvest)
        {
        }
        else if (type == TaskType.Attack)
        {
        }
    }

    // add tasks to this list
    public void CreateTask(TaskType type, string specialization, GameObject position, GameObject returnPoint, int priority)
    {
        Debug.Log("Creating task: " + type);
        if (type == TaskType.Home)
        {
            ITask task = new ReturnHome(returnPoint.transform.position, priority);
            task.SetHomePos = returnPoint.transform;
            taskList.AddTask(task);
        }
        else if (type == TaskType.Idle)
        {
            ITask task = new IdleTask(priority);
            task.SetHomePos = returnPoint.transform;
            taskList.AddTask(task);
        }
        else if (type == TaskType.Scout)
        {
            ITask task = null;
            switch (specialization)
            {
                case "resources":
                    task = new ScoutResources(position.transform.position, priority);
                    task.SetHomePos = returnPoint.transform;
                    taskList.AddTask(task);
                    break;

                default:
                    task = new ScoutResources(position.transform.position, priority);
                    task.SetHomePos = returnPoint.transform;
                    taskList.AddTask(task);
                    break;

            }
        }
        else if (type == TaskType.Harvest)
        {
            ITask task = null;
            switch (specialization)
            {
                case "random":
                    task = new HarvestRandom(position.transform.position, priority);
                    task.SetHomePos = returnPoint.transform;
                    taskList.AddTask(task);
                    break;

                case "specific":
                    task = new HarvestSpecific(target: position, priority: priority);
                    task.SetHomePos = returnPoint.transform;
                    taskList.AddTask(task);
                    break;

                default:
                    task = new HarvestRandom(position.transform.position, priority);
                    task.SetHomePos = returnPoint.transform;
                    taskList.AddTask(task);
                    break;

            }
        }
        else if (type == TaskType.Attack)
        {

        }
        else if (type == TaskType.GoTo)
        {
            ITask task = new GoToTask(position.transform.position, priority);
            task.SetHomePos = returnPoint.transform;
            taskList.AddTask(task);

        }
        Debug.Log("Created task: " + type);
        Debug.Log("Count: " + taskList.GetTaskCount());
    }

    // getter and 'setter' to make default tasks
    public ITask CreateTask(EntityRuntime entity)
{
    // 1. Get current task
    ITask task = entity.taskList.GetCurrentTask();

    // 2. Try pulling from global list if entity has none
    if (task == null)
        task = taskList.GetCurrentTask();

    // 3. Handle idle cleanup
    if (task is IdleTask idle)
    {
        // If entity is idling and new tasks exist, clear idle
        if (taskList.GetTaskCount() > 0 || entity.world.IsUnfoundHarvestables())
        {
            idle.SetIsComplete = true;
            entity.taskList.RemoveTask(idle);
            task = null; // allow replacement
        }
    }

    // 4. Create new default task if needed
    if (task == null)
    {
        switch (entity.behaviour)
        {
            case EntityBehaviour.SCOUT:
                task = new ScoutResources(Vector3.zero, 0);
                break;
            case EntityBehaviour.HARVEST:
                task = new HarvestRandom(Vector3.zero, 0);
                break;
            case EntityBehaviour.DEFAULT:
                task = new ScoutResources(Vector3.zero, 0);
                break;
        }
    }

    // 5. Assign home reference if applicable
    if (entity.home != null)
        task.SetHomePos = entity.home;

    // 6. Choose best available task based on priority
    ITask bestTask = GetLowestPriority(entity, task);

    // 7. Add to entity task list, remove from global if necessary
    entity.taskList.AddTask(bestTask);
    taskList.RemoveTask(bestTask);

    return bestTask;
}



    public void AddTask(ITask task)
    {
        taskList.AddTask(task);
    }

    private ITask GetLowestPriority(EntityRuntime entity, ITask curTask)
    {
        ITask lowestInList = taskList.GetCurrentTask();
        if (lowestInList == null) return curTask;
        if (lowestInList.Priority < curTask.Priority)
        {
            return lowestInList;
        }


        return curTask;
    }
    


}


/* Potential logic fix from chat

public ITask CreateTask(EntityRuntime entity)
{
    // 1. Check entity current task
    ITask currentTask = entity.taskList.GetCurrentTask();

    // 2. Try pulling from global task list if entity has none
    if (currentTask == null)
        currentTask = taskList.GetCurrentTask();

    // 3. Handle idle cleanup
    if (currentTask is IdleTask idle)
    {
        if (taskList.GetTaskCount() > 0 || entity.world.IsUnfoundHarvestables())
        {
            idle.SetIsComplete = true;
            entity.taskList.RemoveTask(idle);
            currentTask = null; // allow new assignment
        }
    }

    // 4. Assign default task only if no global task
    if (currentTask == null)
    {
        if (taskList.GetTaskCount() > 0)
        {
            // Take the first available global task
            currentTask = taskList.GetCurrentTask();
            taskList.RemoveTask(currentTask);
        }
        else
        {
            // No global tasks, assign default based on behaviour
            switch (entity.behaviour)
            {
                case EntityBehaviour.SCOUT:
                    currentTask = new ScoutResources(Vector3.zero, 0);
                    break;
                case EntityBehaviour.HARVEST:
                    currentTask = new HarvestRandom(Vector3.zero, 0);
                    break;
                case EntityBehaviour.DEFAULT:
                    currentTask = new ScoutResources(Vector3.zero, 0);
                    break;
            }
        }
    }

    // 5. Assign home reference if applicable
    if (entity.home != null)
        currentTask.SetHomePos = entity.home;

    // 6. Assign to entity task list
    entity.taskList.AddTask(currentTask);

    return currentTask;
}


bool AssignEntityTasks(EntityRuntime entity, World world)
{
    ITask current = entity.taskList.GetCurrentTask();

    // --- Clean up finished tasks ---
    if (current != null && current.IsComplete)
    {
        entity.taskList.RemoveTask(current);
        current = null;
    }

    bool hasHarvestables = world.IsUnfoundHarvestables();
    bool hasGlobalTasks = taskCreator.taskList.GetTaskCount() > 0;
    bool isAtHome = entity.IsAtHome();

    // --- CASE 1: Global tasks available ---
    if (hasGlobalTasks)
    {
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



*/