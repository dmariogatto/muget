using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MuGet.Forms.UI.Views;
using MuGet.Services;
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

            IoC.Resolve<INuGetService>().Checkpoint();
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
