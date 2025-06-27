using System.Collections.Generic;

namespace SecureBot
{
    public class TaskManager
    {
        public List<TaskItem> Tasks { get; } = new List<TaskItem>();

        public void AddTask(TaskItem task)
        {
            Tasks.Add(task);
        }

        public void DeleteTask(int index)
        {
            if (index >= 0 && index < Tasks.Count)
                Tasks.RemoveAt(index);
        }
    }
}

