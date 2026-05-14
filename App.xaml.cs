using ToDoList.Services;

namespace ToDoList
{
    public partial class App : Application
    {
        private readonly NotificationService? _notificationService;

        public App(NotificationService notificationService)
        {
            InitializeComponent();
            _notificationService = notificationService;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "MyWork" };
        }

        protected override void OnStart()
        {
            base.OnStart();

            // Request permissions and schedule all task notifications
            if (_notificationService != null)
            {
                Task.Run(async () =>
                {
                    var hasPermission = await _notificationService.RequestPermissions();
                    if (hasPermission)
                    {
                        await _notificationService.ScheduleAllTaskNotifications();
                    }
                });
            }
        }
    }
}
