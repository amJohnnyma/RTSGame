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
            var current = entity.currentTask;
            if (current == null || current.Type != TaskType.Home)
            {
                ITask task = new ReturnHome(entity.home.transform.position, 5);
                entity.target = entity.home;
                entity.mainTarget = entity.home;
                entity.currentTask = null;
                entity.currentTask = task;
            }
        }
        else if (type == TaskType.Idle)
        {
            var current = entity.currentTask;
            if (current == null || current.Type != TaskType.Idle)
            {
                ITask task = new IdleTask(9);
                entity.currentTask = null;
                entity.currentTask = task;
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
        // 1. Check entity current task
        ITask currentTask = entity.currentTask;

    // 2. Try pulling from global task list if entity has none
    if (currentTask == null)
        currentTask = taskList.GetCurrentTask();

    // 3. Handle idle cleanup
    if (currentTask is IdleTask idle)
    {
        if (taskList.GetTaskCount() > 0 || entity.world.IsUnfoundHarvestables())
        {
            Debug.Log("Current idle and tasks available");
            idle.SetIsComplete = true;
            entity.currentTask = null;
            currentTask = null; // allow new assignment
        }
    }

    // 4. Assign default task only if no global task
    if (currentTask == null)
    {
        if (taskList.GetTaskCount() > 0)
        {
            Debug.Log("Current null and tasks available");
            // Take the first available global task
            currentTask = taskList.GetCurrentTask();
                Debug.Log("Current Task: " + currentTask.GetType());
            taskList.RemoveTask(currentTask);
        }
        else
        {
            // No global tasks, assign default based on behaviour
            switch (entity.behaviour)
            {
                case EntityBehaviour.SCOUT:
                    currentTask = new ScoutResources(Vector3.zero, 5);
                    break;
                case EntityBehaviour.HARVEST:
                    currentTask = new HarvestRandom(Vector3.zero, 5);
                    break;
                case EntityBehaviour.DEFAULT:
                    currentTask = new ScoutResources(Vector3.zero, 5);
                    break;
            }
        }
    }

    // 5. Assign home reference if applicable
    if (entity.home != null)
        currentTask.SetHomePos = entity.home;

        // 6. Assign to entity task list
        entity.currentTask = currentTask;
        entity.pendingTargetPos = currentTask.TargetPos;

    

    return currentTask;
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

