using MuGet.Forms.UI.Views;
using MuGet.Services;
using System;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Device = Xamarin.Forms.Device;

namespace MuGet.Forms.UI
{
    public partial class App : Application
    {
        static App()
        {
            IoC.RegisterSingleton<IThemeService, ThemeService>();
        }

        public App()
        {
            InitializeComponent();

            VersionTracking.Track();

            Resources.Add("CellDescriptionFontSize", Device.GetNamedSize(NamedSize.Caption, typeof(Label)));

            var localise = IoC.Resolve<ILocalise>();
            var culture = localise.GetCurrentCultureInfo();
            localise.SetLocale(culture);

            if (VersionTracking.IsFirstLaunchEver)
            {
                Shiny.ShinyHost.Resolve<Shiny.Jobs.IJobManager>().Schedule(ShinyStartup.NuGetJobInfo);
            }

            MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts

#if !DEBUG
            Microsoft.AppCenter.AppCenter.Start(
                "android=APPCENTER_ANDROID;" +
                "ios=APPCENTER_IOS;",
                typeof(Microsoft.AppCenter.Analytics.Analytics), typeof(Microsoft.AppCenter.Crashes.Crashes));
#endif

            ThemeManager.LoadTheme();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps

            IoC.Resolve<INuGetService>().Checkpoint();
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        protected override void OnAppLinkRequestReceived(Uri uri)
        {
            base.OnAppLinkRequestReceived(uri);

            var host = uri.Host;
            if (string.Equals(host, "package", StringComparison.OrdinalIgnoreCase))
            {
                var segments = uri.Segments
                    .Select(i => i.TrimEnd('/'))
                    .Where(i => !string.IsNullOrWhiteSpace(i))
                    .ToList();
                var packageId = segments.Count > 0 ? segments[0] : string.Empty;
                var version = segments.Count > 1 ? segments[1] : string.Empty;

                if (!string.IsNullOrEmpty(packageId) &&
                    MainPage is NavigationPage navPage)
                {
                    var packagePage = new PackagePage
                    {
                        PackageId = packageId,
                        Version = version
                    };

                    navPage.PushAsync(packagePage);
                }
            }
        }
    }
}