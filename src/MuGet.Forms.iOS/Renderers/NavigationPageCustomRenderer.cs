using Foundation;
using MuGet.Forms.iOS.Renderers;
using MuGet.Forms.UI;
using MuGet.Services;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(NavigationPage), typeof(NavigationPageCustomRenderer))]
namespace MuGet.Forms.iOS.Renderers
{
    [Preserve(AllMembers = true)]
    public class NavigationPageCustomRenderer : NavigationRenderer
    {
        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);

            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                var oldTheme = previousTraitCollection?.UserInterfaceStyle ?? UIUserInterfaceStyle.Light;
                var newTheme = TraitCollection?.UserInterfaceStyle ?? UIUserInterfaceStyle.Light;

                if (oldTheme != newTheme)
                {
                    switch (newTheme)
                    {
                        case UIUserInterfaceStyle.Dark:
                            ThemeManager.ChangeTheme(Theme.Dark);
                            break;
                        default:
                            ThemeManager.ChangeTheme(Theme.Light);
                            break;
                    }
                }
            }
        }
    }
}