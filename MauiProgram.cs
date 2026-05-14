using Microsoft.Extensions.Logging;
using ToDoList.Services;
using Plugin.LocalNotification;

namespace ToDoList
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            // Register services
            builder.Services.AddSingleton<TaskService>();
            builder.Services.AddSingleton<NotificationService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // Request notification permissions and schedule notifications on app startup
            Task.Run(async () =>
            {
                var notificationService = app.Services.GetRequiredService<NotificationService>();

                // Request permissions
                var hasPermission = await notificationService.RequestPermissions();
                System.Diagnostics.Debug.WriteLine($"Notification permission: {hasPermission}");

                // Schedule all task notifications
                if (hasPermission)
                {
                    await notificationService.ScheduleAllTaskNotifications();
                }
            });

            return app;
        }
    }
}
