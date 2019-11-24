using MuGet.Forms.Models;
using MuGet.Forms.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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

        private void DependencyTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is Dependency dependency)
            {
                ViewModel.DependancyTappedCommand.Execute(dependency);
            }
        }

        private void VersionTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is CatalogEntry entry)
            {
                ViewModel.EntryTappedCommand.ExecuteAsync(entry);
            }
        }        
    }
}