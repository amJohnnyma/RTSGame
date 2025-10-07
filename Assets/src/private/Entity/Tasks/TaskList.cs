using System.Collections.Generic;
using System.Linq;

public class TaskList
{
    private readonly List<ITask> _tasks = new();

    public void AddTask(ITask task) => _tasks.Add(task);
    public void RemoveTask(ITask task) => _tasks.Remove(task);
    public void ClearTasks() => _tasks.Clear();

    public ITask? GetCurrentTask()
    {
        if (_tasks.Count == 0) return null;

        return _tasks.OrderByDescending(t => t.Priority).First();
    }

    public int GetTaskCount()
    {
        return _tasks.Count;
    }
}
