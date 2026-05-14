using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using Plugin.LocalNotification.iOSOption;
using ToDoList.Models;

namespace ToDoList.Services
{
    public class NotificationService
    {
        private readonly TaskService _taskService;

        public NotificationService(TaskService taskService)
        {
            _taskService = taskService;
            _taskService.OnTasksChanged += OnTasksChanged;
        }

        // This gets called whenever tasks change (add/edit/delete/complete)
        private void OnTasksChanged()
        {
            // Re-schedule all notifications when tasks change
            _ = ScheduleAllTaskNotifications();
        }

        /// <summary>
        /// Schedules notifications for all upcoming tasks
        /// This runs in the background and sends notifications even when app is closed
        /// </summary>
        public async Task ScheduleAllTaskNotifications()
        {
            try
            {
                // Cancel all existing notifications first
                LocalNotificationCenter.Current.CancelAll();

                var pendingTasks = _taskService.GetPendingTasks();

                foreach (var task in pendingTasks)
                {
                    await ScheduleTaskNotifications(task);
                }

                System.Diagnostics.Debug.WriteLine($"Scheduled notifications for {pendingTasks.Count} tasks");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error scheduling notifications: {ex.Message}");
            }
        }

        /// <summary>
        /// Schedules multiple notifications for a single task:
        /// - 3 days before due date
        /// - 1 day before due date
        /// - On due date (morning)
        /// - On due date (if still pending)
        /// </summary>
        private async Task ScheduleTaskNotifications(TodoTask task)
        {
            try
            {
                var now = DateTime.Now;
                var dueDate = task.ExpectedDelivery;

                // Notification 1: 3 days before (at 9 AM)
                var threeDaysBefore = dueDate.AddDays(-3).Date.AddHours(9);
                if (threeDaysBefore > now)
                {
                    await ScheduleNotification(
                        task.Id.GetHashCode() + 1,
                        "📅 Task Due in 3 Days",
                        $"'{task.TaskName}' is due in 3 days ({dueDate:MMM dd})",
                        threeDaysBefore
                    );
                }

                // Notification 2: 1 day before (at 9 AM)
                var oneDayBefore = dueDate.AddDays(-1).Date.AddHours(9);
                if (oneDayBefore > now)
                {
                    await ScheduleNotification(
                        task.Id.GetHashCode() + 2,
                        "⚠️ Task Due Tomorrow",
                        $"'{task.TaskName}' is due tomorrow!",
                        oneDayBefore
                    );
                }

                // Notification 3: On due date morning (at 8 AM)
                var dueDateMorning = dueDate.Date.AddHours(8);
                if (dueDateMorning > now)
                {
                    await ScheduleNotification(
                        task.Id.GetHashCode() + 3,
                        "🔥 Task Due Today!",
                        $"'{task.TaskName}' is due today. Don't forget!",
                        dueDateMorning
                    );
                }

                // Notification 4: On due date afternoon reminder (at 2 PM)
                var dueDateAfternoon = dueDate.Date.AddHours(14);
                if (dueDateAfternoon > now && !task.IsCompleted)
                {
                    await ScheduleNotification(
                        task.Id.GetHashCode() + 4,
                        "🚨 Urgent: Task Due Today",
                        $"'{task.TaskName}' is due today. Please complete it!",
                        dueDateAfternoon
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error scheduling task notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Schedules a single notification at a specific time
        /// Works even when app is closed! (Cross-platform: Android & iOS)
        /// </summary>
        private async Task ScheduleNotification(int id, string title, string message, DateTime scheduledTime)
        {
            try
            {
                var request = new NotificationRequest
                {
                    NotificationId = id,
                    Title = title,
                    Description = message,
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = scheduledTime
                    },
                    // Android-specific settings
                    Android = new AndroidOptions
                    {
                        Priority = AndroidPriority.High,
                        VisibilityType = AndroidVisibilityType.Public,
                        AutoCancel = true
                    },
                    // iOS-specific settings
                    iOS = new iOSOptions
                    {
                        HideForegroundAlert = false,
                        PlayForegroundSound = true
                    }
                };

                await LocalNotificationCenter.Current.Show(request);
                System.Diagnostics.Debug.WriteLine($"Scheduled notification '{title}' for {scheduledTime}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows an immediate notification (for testing or urgent alerts)
        /// Cross-platform: Android & iOS
        /// </summary>
        public async Task ShowImmediateNotification(string title, string message)
        {
            try
            {
                var request = new NotificationRequest
                {
                    NotificationId = new Random().Next(),
                    Title = title,
                    Description = message,
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = DateTime.Now.AddSeconds(1)
                    },
                    Android = new AndroidOptions
                    {
                        Priority = AndroidPriority.High,
                        AutoCancel = true
                    },
                    iOS = new iOSOptions
                    {
                        HideForegroundAlert = false,
                        PlayForegroundSound = true
                    }
                };

                await LocalNotificationCenter.Current.Show(request);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing immediate notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Request notification permissions (must be called on app startup)
        /// Cross-platform: Android & iOS
        /// </summary>
        public async Task<bool> RequestPermissions()
        {
            try
            {
                var result = await LocalNotificationCenter.Current.AreNotificationsEnabled();
                if (!result)
                {
                    await LocalNotificationCenter.Current.RequestNotificationPermission();
                }
                return await LocalNotificationCenter.Current.AreNotificationsEnabled();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error requesting permissions: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Old method - kept for backward compatibility
        /// Now just calls ScheduleAllTaskNotifications
        /// </summary>
        public async Task CheckAndScheduleNotifications()
        {
            await ScheduleAllTaskNotifications();
        }
    }
}
