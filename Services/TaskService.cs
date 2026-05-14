using System.Text.Json;
using ToDoList.Models;

namespace ToDoList.Services
{
    public class TaskService
    {
        private List<TodoTask> _tasks = new();
        private const string StorageKey = "tasks";

        public event Action? OnTasksChanged;

        public TaskService()
        {
            LoadTasks();
        }

        public List<TodoTask> GetAllTasks()
        {
            return _tasks.OrderBy(t => t.IsCompleted)
                         .ThenBy(t => t.ExpectedDelivery)
                         .ToList();
        }

        public List<TodoTask> GetPendingTasks()
        {
            return _tasks.Where(t => !t.IsCompleted)
                         .OrderBy(t => t.ExpectedDelivery)
                         .ToList();
        }

        public List<TodoTask> GetTasksDueToday()
        {
            return _tasks.Where(t => t.IsDueToday).ToList();
        }

        public List<TodoTask> GetTasksDueInNext3Days()
        {
            return _tasks.Where(t => t.IsDueInNext3Days).ToList();
        }

        public List<TodoTask> GetOverdueTasks()
        {
            return _tasks.Where(t => t.IsOverdue).ToList();
        }

        public void AddTask(TodoTask task)
        {
            _tasks.Add(task);
            SaveTasks();
            OnTasksChanged?.Invoke();
        }

        public void UpdateTask(TodoTask task)
        {
            var existingTask = _tasks.FirstOrDefault(t => t.Id == task.Id);
            if (existingTask != null)
            {
                _tasks.Remove(existingTask);
                _tasks.Add(task);
                SaveTasks();
                OnTasksChanged?.Invoke();
            }
        }

        public void DeleteTask(Guid taskId)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                _tasks.Remove(task);
                SaveTasks();
                OnTasksChanged?.Invoke();
            }
        }

        public void ToggleTaskCompletion(Guid taskId)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                task.IsCompleted = !task.IsCompleted;
                task.CompletedDate = task.IsCompleted ? DateTime.Now : null;
                SaveTasks();
                OnTasksChanged?.Invoke();
            }
        }

        public Dictionary<string, int> GetCategoryStatistics()
        {
            return new Dictionary<string, int>
            {
                { "Total", _tasks.Count },
                { "Pending", _tasks.Count(t => !t.IsCompleted) },
                { "Completed", _tasks.Count(t => t.IsCompleted) },
                { "Overdue", _tasks.Count(t => t.IsOverdue) },
                { "Due Today", _tasks.Count(t => t.IsDueToday) },
                { "PD Tasks", _tasks.Count(t => t.Category == TaskCategory.PD) },
                { "PS Tasks", _tasks.Count(t => t.Category == TaskCategory.PS) },
                { "Personal Tasks", _tasks.Count(t => t.Category == TaskCategory.Personal) }
            };
        }

        private void SaveTasks()
        {
            try
            {
                var json = JsonSerializer.Serialize(_tasks);
                Preferences.Default.Set(StorageKey, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving tasks: {ex.Message}");
            }
        }

        private void LoadTasks()
        {
            try
            {
                var json = Preferences.Default.Get(StorageKey, string.Empty);
                if (!string.IsNullOrEmpty(json))
                {
                    _tasks = JsonSerializer.Deserialize<List<TodoTask>>(json) ?? new List<TodoTask>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading tasks: {ex.Message}");
                _tasks = new List<TodoTask>();
            }
        }
    }
}
