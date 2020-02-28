using MuGet.Forms.Models;
using MuGet.Forms.ViewModels;
using System.Linq;
using System.Net;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;
using Application = Xamarin.Forms.Application;

namespace MuGet.Forms.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PackagePage : BasePage<PackageViewModel>
    {
        public PackagePage() : base()
        {
            InitializeComponent();

            Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);                     
        }        

        public string PackageId
        {
            get => ViewModel.PackageId;
            set
            {
                ViewModel.PackageId = WebUtility.UrlDecode(value);
            }
        }

        public string Version
        {
            get => ViewModel.Version;
            set
            {
                ViewModel.Version = WebUtility.UrlDecode(value);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Device.RuntimePlatform == Device.Android)
            {
                SegPageControl.SelectedTextColor = (Color)Application.Current.Resources["ContrastAntiColor"];
                SegPageControl.TintColor = (Color)Application.Current.Resources["ContrastColor"];
            }

            if (Navigation.NavigationStack.LastOrDefault() == this &&
                Navigation.NavigationStack.Count > 1 &&
                Navigation.NavigationStack[Navigation.NavigationStack.Count - 2] is ContentPage previous)
            {
                BackButton.Text = previous.Title;
                CloseButton.IsVisible = Navigation.NavigationStack.Count > 2;
            }
        }


        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();

            if (Device.RuntimePlatform == Device.iOS &&
                Navigation.NavigationStack.FirstOrDefault() is ContentPage rootPage)
            {
                // Want the header "card" to extend to the top screen edge
                var safeInsets = rootPage.On<Xamarin.Forms.PlatformConfiguration.iOS>().SafeAreaInsets();
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

                    // For iOS, resize scroll view content size for different packages
                    DetailsView.ForceLayout();
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

        private void DependencyTapped(object sender, System.EventArgs e)
        {
            if (sender is View v && v.BindingContext is Dependency dependency)
            {
                var packagePage = new PackagePage();
                packagePage.PackageId = dependency.Id;
                packagePage.Version = dependency.VersionRange?.MinVersion != null
                    ? dependency.VersionRange.MinVersion.ToString()
                    : string.Empty;

                Navigation.PushAsync(packagePage);
            }
        }

        private void BackClicked(object sender, System.EventArgs e)
        {            
            Navigation.PopAsync();
        }

        private void CloseClicked(object sender, System.EventArgs e)
        {
            Navigation.PopToRootAsync();
        }
    }
}