using MuGet.Forms.Models;
using MuGet.Forms.ViewModels;
using System.Linq;
using System.Net;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

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

        private void SelectedTabIndexChanged(object sender, SelectedPositionChangedEventArgs e)
        {
            switch (e.SelectedPosition)
            {
                case 0:
                    PackageDetails.IsVisible = true;
                    Dependancies.IsVisible = false;
                    Versions.IsVisible = false;

                    // For iOS, resize scroll view content size for different packages
                    PackageDetails.ForceLayout();
                    break;
                case 1:
                    PackageDetails.IsVisible = false;
                    Dependancies.IsVisible = true;
                    Versions.IsVisible = false;
                    break;
                case 2:
                    PackageDetails.IsVisible = false;
                    Dependancies.IsVisible = false;
                    Versions.IsVisible = true;
                    break;
                default:
                    break;
            }
        }

        private void DependencyTapped(object sender, System.EventArgs e)
        {
            if (sender is View v && v.BindingContext is Dependency dependency)
            {
                var packagePage = new PackagePage
                {
                    PackageId = dependency.Id,
                    Version = dependency.VersionRange?.MinVersion != null
                        ? dependency.VersionRange.MinVersion.ToString()
                        : string.Empty
                };

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