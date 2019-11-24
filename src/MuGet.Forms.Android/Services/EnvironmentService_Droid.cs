using System;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using MuGet.Forms.Services;
using Plugin.CurrentActivity;
using Xamarin.Forms;

[assembly: Dependency(typeof(EnvironmentService_Droid))]
namespace MuGet.Forms.Services
{
    [Preserve(AllMembers = true)]
    public class EnvironmentService_Droid : IEnvironmentService
    {
        // manual dark mode for the moment
        public bool NativeDarkMode => (int)Build.VERSION.SdkInt >= 29; // Q

        public Theme GetOperatingSystemTheme()
        {
            var uiModeFlags = CrossCurrentActivity.Current.AppContext.Resources.Configuration.UiMode & UiMode.NightMask;
            switch (uiModeFlags)
            {
                case UiMode.NightYes:
                    return Theme.Dark;
                case UiMode.NightNo:
                    return Theme.Light;
                default:
                    throw new NotSupportedException($"UiMode {uiModeFlags} not supported");
            }
        }
    }
}