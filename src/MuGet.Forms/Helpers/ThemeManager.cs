using MuGet.Forms.Services;
using MuGet.Forms.Styles;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace MuGet.Forms
{
    public enum Theme
    {
        Light,
        Dark,
    }
    
    public static class ThemeManager
    {
        private readonly static IDictionary<Theme, ResourceDictionary> _resources = new Dictionary<Theme, ResourceDictionary>();

        public static event EventHandler<Theme> ThemeChanged;

        public static Theme Current =>
            (Theme)Xamarin.Essentials.Preferences.Get(nameof(Theme), 0);

        public static void LoadTheme()
        {
            var environmentService = DependencyService.Resolve<IEnvironmentService>();
            if (environmentService.NativeDarkMode)
            {
                var theme = environmentService.GetOperatingSystemTheme();
                ChangeTheme(theme);
            }
            else
            {
                ChangeTheme(Current);
            }
            
        }

        public static void ChangeTheme(Theme theme)
        {
            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            if (mergedDictionaries != null)
            {
                mergedDictionaries.Clear();
                mergedDictionaries.Add(GetDictionary(theme));

                if (theme != Current)
                    ThemeChanged?.Invoke(Application.Current, theme);

                Xamarin.Essentials.Preferences.Set(nameof(Theme), (int)theme);
            }
        }

        private static ResourceDictionary GetDictionary(Theme theme)
        {
            if (!_resources.ContainsKey(theme))
            {
                switch (theme)
                {
                    case Theme.Dark:
                        _resources.Add(Theme.Dark, new DarkTheme());
                        break;
                    default:
                        _resources.Add(Theme.Light, new LightTheme());
                        break;
                }
            }

            return _resources[theme];
        }
    }
}
