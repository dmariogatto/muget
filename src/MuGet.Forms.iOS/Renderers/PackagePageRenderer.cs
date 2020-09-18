using Foundation;
using MuGet.Forms.iOS.Renderers;
using MuGet.Forms.UI.Views;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(PackagePage), typeof(PackagePageRenderer))]
namespace MuGet.Forms.iOS.Renderers
{
    [Preserve(AllMembers = true)]
    public class PackagePageRenderer : PageRenderer
    {
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (NavigationController != null)
            {
                var view = new UIView() { Hidden = true };
                NavigationController.NavigationBar.TopItem.TitleView = view;
            }
        }
    }
}