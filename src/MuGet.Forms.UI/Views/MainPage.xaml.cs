using MuGet.Forms.UI.Extentions;
using MuGet.Models;
using MuGet.ViewModels;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace MuGet.Forms.UI.Views
{
    public partial class MainPage : BasePage<MainViewModel>
    {
        private PackagePage _packagePage;

        public MainPage() : base()
        {
            InitializeComponent();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            HomeView.Margin =
                SkeletonView.Margin =
                    SearchCollectionHeaderView.Margin =
                new Thickness(0, SearchBarView.Height + SearchBarView.Margin.Top + 4, 0, 0);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Task.WhenAll(
                SearchBarView.TranslateTo(0, 0, 250, Easing.CubicOut),
                SearchBarView.FadeTo(1, 200));
        }

        private void PackagesScrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            var transY = Convert.ToInt32(SearchBarView.TranslationY);
            if (transY == 0 &&
                e.VerticalDelta > 15)
            {
                var trans = SearchBarView.Height + SearchBarView.Margin.Top;
                var safeInsets = On<Xamarin.Forms.PlatformConfiguration.iOS>().SafeAreaInsets();

                Task.WhenAll(
                    SearchBarView.TranslateTo(0, -(trans + safeInsets.Top), 250, Easing.CubicIn),
                    SearchBarView.FadeTo(0.25, 200));
            }
            else if (transY != 0 &&
                     e.VerticalDelta < 0 &&
                     Math.Abs(e.VerticalDelta) > 10)
            {
                Task.WhenAll(
                    SearchBarView.TranslateTo(0, 0, 250, Easing.CubicOut),
                    SearchBarView.FadeTo(1, 200));
            }
        }

        private void PackageTapped(object sender, EventArgs e)
        {
            if (sender is View v && v.BindingContext is PackageMetadata metadata)
            {
                _ = Navigation.PushPageFactoryAsync(() =>
                {
                    _packagePage ??= new PackagePage();
                    _packagePage.PackageId = metadata.Id;
                    _packagePage.Version = metadata.Version;
                    return _packagePage;
                });
            }
        }
    }
}