using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskCreator : MonoBehaviour
{

    [SerializeField] private World world; // reference world -> Which planet are we one
    [SerializeField] private GameObject home; // Which home are we returning to?

    private List<ITask> taskList = new();

// create a task for this entity
    public void CreateTask(TaskType type, string specialization, EntityRuntime entity)
{
    if (type == TaskType.Home)
    {
        var current = entity.taskList.GetCurrentTask();
        if (current == null || current.Type != TaskType.Home)
        {
            ITask task = new ReturnHome(entity.home.transform.position, 1);
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
            ITask task = new IdleTask();
            entity.taskList.ClearTasks();
            entity.taskList.AddTask(task);
        }
    }
    else
    {
        // ... other tasks like Scout/Harvest
    }
}

    public ITask CreateTask(EntityRuntime entity)
    {
        ITask task = entity.taskList.GetCurrentTask();
        if (task == null) // entity doesnt have any tasks
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

        // else check if the priority is the lowest of this task
        ITask bestTask = GetLowestPriority(entity, task);
        // only add the task if needed
        entity.taskList.AddTask(bestTask);

        return bestTask;

    }

    private ITask GetLowestPriority(EntityRuntime entity, ITask curTask)
    {
        foreach (ITask t in taskList)
        {
       
            if (t.Priority < curTask.Priority)
            {
                return t;
            }

            
        }

        return curTask;
    }
}
