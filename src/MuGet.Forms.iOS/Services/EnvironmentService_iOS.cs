using Foundation;
using System;
using UIKit;
using Xamarin.Forms;

namespace MuGet.Services
{
    [Preserve(AllMembers = true)]
    public class EnvironmentService_iOS : IEnvironmentService
    {
        // manual dark mode for the moment
        public bool NativeDarkMode => UIDevice.CurrentDevice.CheckSystemVersion(13, 0);

        public Theme GetOperatingSystemTheme()
        {
            var theme = Theme.Light;
            
            // 'TraitCollection.UserInterfaceStyle' was introduced in iOS 12.0
            if (UIDevice.CurrentDevice.CheckSystemVersion(12, 0))
            {
                try
                {
                    var currentUIViewController = GetVisibleViewController();
                    var userInterfaceStyle = currentUIViewController.TraitCollection.UserInterfaceStyle;

                    switch (userInterfaceStyle)
                    {
                        case UIUserInterfaceStyle.Light:
                            theme = Theme.Light;
                            break;
                        case UIUserInterfaceStyle.Dark:
                            theme = Theme.Dark;
                            break;
                        default:
                            throw new NotSupportedException($"UIUserInterfaceStyle {userInterfaceStyle} not supported");
                    }
                }
                catch (Exception ex)
                {
                    var logger = DependencyService.Get<ILogger>();
                    logger.Error(ex);
                }
            }
            
            return theme;
        }
        
        private static UIViewController GetVisibleViewController()
        {
            var rootController = UIApplication.SharedApplication.KeyWindow.RootViewController;

            switch (rootController.PresentedViewController)
            {
                case UINavigationController navigationController:
                    return navigationController.TopViewController;
                case UITabBarController tabBarController:
                    return tabBarController.SelectedViewController;
                case null:
                    return rootController;
                default:
                    return rootController.PresentedViewController;
            }
        }
    }
}