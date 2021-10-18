using MuGet.Forms.UI.Styles;
using MuGet.Services;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace MuGet.Forms.UI
{
    public static class ThemeManager
    {
        private readonly static IDictionary<Theme, ResourceDictionary> Resources = new Dictionary<Theme, ResourceDictionary>();

        public static event EventHandler<Theme> ThemeChanged;

        public static Theme Current =>
            (Theme)Xamarin.Essentials.Preferences.Get(nameof(Theme), 0);

        public static void LoadTheme()
        {
            var environmentService = IoC.Resolve<IEnvironmentService>();
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
            if (!Resources.ContainsKey(theme))
            {
                switch (theme)
                {
                    case Theme.Dark:
                        Resources.Add(Theme.Dark, new DarkTheme());
                        break;
                    default:
                        Resources.Add(Theme.Light, new LightTheme());
                        break;
                }
            }

            return Resources[theme];
        }
    }
}
