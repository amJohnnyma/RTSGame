using System.Collections.Generic;
using System.Linq;

public class TaskList
{
    private readonly List<BaseTask> _tasks = new();

    public void AddTask(BaseTask task) => _tasks.Add(task);
    public void RemoveTask(BaseTask task) => _tasks.Remove(task);
    public void ClearTasks() => _tasks.Clear();



    public int GetTaskCount()
    {
        return _tasks.Count;
    }
}
