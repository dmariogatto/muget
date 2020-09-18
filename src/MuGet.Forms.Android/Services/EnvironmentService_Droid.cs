using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using System;

namespace MuGet.Services
{
    [Preserve(AllMembers = true)]
    public class EnvironmentService_Droid : IEnvironmentService
    {
        // manual dark mode for the moment
        public bool NativeDarkMode => (int)Build.VERSION.SdkInt >= 29; // Q

        public Theme GetOperatingSystemTheme()
        {
            var uiModeFlags = Xamarin.Essentials.Platform.AppContext.Resources.Configuration.UiMode & UiMode.NightMask;
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