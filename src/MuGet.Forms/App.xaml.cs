using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MuGet.Forms.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MuGet.Forms
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            VersionTracking.Track();

            DependencyService.Register<ILogger, Logger>();
            DependencyService.Register<ICacheProvider, InMemoryCache>();
            DependencyService.Register<INuGetService, NuGetService>();
            
            var localise = DependencyService.Get<ILocalise>();
            var culture = localise.GetCurrentCultureInfo();
            localise.SetLocale(culture);            

            if (VersionTracking.IsFirstLaunchEver)
            {
                Shiny.ShinyHost.Resolve<Shiny.Jobs.IJobManager>().Schedule(ShinyStartup.NuGetJobInfo);
            }

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            // Handle when your app starts

#if !DEBUG
            AppCenter.Start(
                "android=APPCENTER_ANDROID;" +
                "ios=APPCENTER_IOS;",
                typeof(Analytics), typeof(Crashes));
#endif

            ThemeManager.LoadTheme();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
