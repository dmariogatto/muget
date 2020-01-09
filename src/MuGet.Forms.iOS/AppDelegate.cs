using System;
using System.Linq;
using System.Net;
using Foundation;
using MuGet.Forms.Views;
using UIKit;
using UserNotifications;
using Xamarin.Forms;

namespace MuGet.Forms.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
#if DEBUG
            UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(UIApplication.BackgroundFetchIntervalMinimum);
#else
            UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(60 * 60 * 3); // 3 hours
#endif

            Shiny.iOSShinyHost.Init(new ShinyStartup());
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            AiForms.Renderers.iOS.SettingsViewInit.Init();

            global::Xamarin.Forms.Forms.Init();

            Plugin.Segmented.Control.iOS.SegmentedControlRenderer.Initialize();

            LoadApplication(new App());

            UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge, (b, e) =>
            {
                if (!b)
                    System.Diagnostics.Debug.WriteLine($"Notification Request Error: {e}");
            });            

            return base.FinishedLaunching(app, options);
        }

        public override void OnActivated(UIApplication uiApplication)
        {
            base.OnActivated(uiApplication);

            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
        }

        public override void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
        {
            Shiny.Jobs.JobManager.OnBackgroundFetch(completionHandler);       
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            var host = url.Host;
            var pathSegs = url.PathComponents.Where(c => c != "/").ToList();
            if (string.Equals(host, "package", StringComparison.OrdinalIgnoreCase))
            {
                var packageId = pathSegs.Count > 0 ? pathSegs[0] : string.Empty;
                var version = pathSegs.Count > 1 ? pathSegs[1] : string.Empty;

                if (!string.IsNullOrEmpty(packageId) &&
                    Xamarin.Forms.Application.Current.MainPage is NavigationPage navPage)
                {
                    var packagePage = new PackagePage();
                    packagePage.PackageId = packageId;

                    if (!string.IsNullOrEmpty(version))
                        packagePage.Version = version;

                    Device.InvokeOnMainThreadAsync(() => navPage.PushAsync(packagePage));
                }
            }

            return true;
        }
    }
}
