using System.Collections.Generic;
using System.Linq;

public class TaskList
{
    private readonly List<ITask> _tasks = new();

    public void AddTask(ITask task) => _tasks.Add(task);
    public void RemoveTask(ITask task) => _tasks.Remove(task);
    public void ClearTasks() => _tasks.Clear();



    public int GetTaskCount()
    {
        return _tasks.Count;
    }
}
