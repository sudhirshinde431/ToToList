using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;

namespace ToDoList
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Enable edge-to-edge display
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                Window?.SetDecorFitsSystemWindows(false);
            }
            else if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
#pragma warning disable CA1422
                Window?.DecorView.SystemUiVisibility = (Android.Views.StatusBarVisibility)(
                    (int)Android.Views.SystemUiFlags.LayoutStable |
                    (int)Android.Views.SystemUiFlags.LayoutFullscreen);
#pragma warning restore CA1422
            }
        }
    }
}
