using System.Net;
using MuGet.Forms.Models;
using MuGet.Forms.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;
using Application = Xamarin.Forms.Application;

namespace MuGet.Forms.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(PackageId), PackageIdUrlQueryProperty)]
    [QueryProperty(nameof(Version), VersionQueryProperty)]
    public partial class PackagePage : BasePage<PackageViewModel>
    {
        public const string RouteName = "package";
        public const string PackageIdUrlQueryProperty = "id";
        public const string VersionQueryProperty = "v";

        public PackagePage()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.Android)
            {
                SegPageControl.SelectedTextColor = (Color)Application.Current.Resources["ContrastAntiColor"];
                SegPageControl.TintColor = (Color)Application.Current.Resources["ContrastColor"];
            }
        }

        public string PackageId
        {
            set
            {
                ViewModel.PackageId = WebUtility.UrlDecode(value);
            }
        }

        public string Version
        {
            set
            {
                ViewModel.Version = WebUtility.UrlDecode(value);
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (Device.RuntimePlatform == Device.iOS)
            {
                // Apply insets otherwise loader is displayed behind tabbar
                var safeInsets = On<Xamarin.Forms.PlatformConfiguration.iOS>().SafeAreaInsets();
                var padding = HeaderView.Padding;
                padding.Top = safeInsets.Top;
                HeaderView.Padding = padding;
                
            }
        }

        private void OnSegmentSelected(object sender, Plugin.Segmented.Event.SegmentSelectEventArgs e)
        {
            switch (e.NewValue)
            {
                case 0:
                    DetailsView.IsVisible = true;
                    DependenciesView.IsVisible = false;
                    VersionsView.IsVisible = false;
                    break;
                case 1:
                    DetailsView.IsVisible = false;
                    DependenciesView.IsVisible = true;
                    VersionsView.IsVisible = false;
                    break;
                case 2:
                    DetailsView.IsVisible = false;
                    DependenciesView.IsVisible = false;
                    VersionsView.IsVisible = true;
                    break;
                default:
                    break;
            }
        }
    }
}