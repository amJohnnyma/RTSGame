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
                ITask task = new ReturnHome(entity.home.transform.position, 9);
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
        ITask task = entity.taskList.GetCurrentTask();
        if (task == null)
        {
            task = taskList.GetCurrentTask();
        }
        if (task == null) // entity doesnt have any tasks // and no tasks available
        {
            // else assign a default task
            switch (entity.behaviour)
            {
                case EntityBehaviour.SCOUT:
                    task = new ScoutResources(Vector3.zero, 9);
                    break;
                case EntityBehaviour.HARVEST:
                    task = new HarvestRandom(Vector3.zero, 9);
                    break;
                case EntityBehaviour.DEFAULT:
                    task = new ScoutResources(Vector3.zero, 9);
                    break;
            }


        }
        else
        {
            IdleTask idleTask = task as IdleTask;
            // not idle so we can just continue
            if (idleTask == null)
            {
                // break out and get the best task
            }
            // is idle so lets check if it is completed
            else if (idleTask.IsIdling)
            {
                idleTask.SetIsComplete = true;
                entity.taskList.ClearTasks(); // we have completed it so remove it from the list
                CreateTask(entity); // retry
                                                    // continue to find a better task
            }
        }
        // now try and assign the values

        // else check if the priority is the lowest of this task
        ITask bestTask = GetLowestPriority(entity, task);
        // only add the task if needed
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
    
    public void AssignAvailableTasksToIdleEntities(List<EntityRuntime> entities)
{
    if (taskList.GetTaskCount() == 0) return;

    for (int i = 0; i < entities.Count; i++)
    {
        var entity = entities[i];
        ITask current = entity.taskList.GetCurrentTask();

        // Skip if entity is busy or has an active non-idle task
        if (current != null && current.Type != TaskType.Idle && !current.IsComplete)
            continue;

        // Take the highest-priority task from the global list
        ITask newTask = taskList.GetCurrentTask();
        if (newTask == null) continue;

        // Assign it
        entity.taskList.ClearTasks();
        entity.taskList.AddTask(newTask);

        // Set target
        entity.pendingTargetPos = newTask.TargetPos;
        entity.target = null; // Force re-evaluation in movement
        entity.mainTarget = null;

        // Remove from global pool
        taskList.RemoveTask(newTask);

        Debug.Log($"Assigned {newTask.Type} to {entity.name}");
    }
}

}


/*
using UnityEngine;

public class TaskCreator : MonoBehaviour
{
    [SerializeField] private World world;
    public TaskList GlobalTasks { get; private set; } = new();

    public void CreateGlobalTask(TaskType type, string specialization, GameObject position, GameObject returnPoint, int priority)
    {
        ITask task = BuildTask(type, specialization, position, returnPoint, priority);
        if (task == null) return;

        GlobalTasks.AddTask(task);
        Debug.Log($"[TaskCreator] Added global {type} task. Total = {GlobalTasks.GetTaskCount()}");
    }

    public void AssignDirectTask(EntityRuntime entity, TaskType type, string specialization, GameObject position, GameObject returnPoint, int priority)
    {
        ITask task = BuildTask(type, specialization, position, returnPoint, priority);
        if (task == null) return;

        entity.taskList.ClearTasks();
        entity.taskList.AddTask(task);
        entity.SetTaskTargets(task);

        Debug.Log($"[TaskCreator] Assigned {type} task directly to {entity.name}");
    }

    private ITask BuildTask(TaskType type, string specialization, GameObject position, GameObject returnPoint, int priority)
    {
        ITask task = type switch
        {
            TaskType.Home => new ReturnHome(returnPoint.transform.position, priority),
            TaskType.Idle => new IdleTask(priority),
            TaskType.Scout when specialization == "resources" => new ScoutResources(position.transform.position, priority),
            TaskType.Harvest when specialization == "random" => new HarvestRandom(position.transform.position, priority),
            TaskType.Harvest when specialization == "specific" => new HarvestSpecific(position, priority),
            TaskType.GoTo => new GoToTask(position.transform.position, priority),
            _ => null
        };

        if (task != null)
            task.SetHomePos = returnPoint?.transform;

        return task;
    }
}

*/