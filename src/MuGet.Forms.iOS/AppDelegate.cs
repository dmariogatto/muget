﻿using Foundation;
using MuGet.Forms.iOS.Services;
using MuGet.Forms.UI;
using MuGet.Forms.UI.Services;
using MuGet.Services;
using System;
using UIKit;
using Xamarin.Forms;

[assembly: ResolutionGroupName("MuGet.Effects")]

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
            IoC.RegisterSingleton<ILocalise, LocaliseService_iOS>();
            IoC.RegisterSingleton<IEnvironmentService, EnvironmentService_iOS>();
            IoC.RegisterSingleton<IRendererResolver, RendererResolver_iOS>();
            IoC.RegisterSingleton<IHttpHandlerService, HttpHandlerService_iOS>();
            IoC.RegisterSingleton<IThemeService, ThemeService>();

            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            AiForms.Renderers.iOS.SettingsViewInit.Init();

            global::Xamarin.Forms.Forms.Init();

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            if (Xamarin.Forms.Application.Current != null)
            {
                var uri = new Uri(url.ToString());
                Xamarin.Forms.Application.Current.SendOnAppLinkRequestReceived(uri);

                return true;
            }

            return false;
        }

        public override void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler)
        {
            Xamarin.Essentials.Platform.PerformActionForShortcutItem(application, shortcutItem, completionHandler);
        }
    }
}
