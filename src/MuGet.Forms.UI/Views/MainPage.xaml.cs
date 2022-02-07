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
        public MainPage() : base()
        {
            InitializeComponent();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (width <= 0 || height <= 0)
                return;

            SearchBarView.WidthRequest = Device.Idiom == TargetIdiom.Tablet
                ? width / 1.8d
                : width / 1.4d;

            HomeScrollView.Padding =
                PackagesSkeletonView.Padding =
                    PackagesCollectionViewHeader.Padding =
                        new Thickness(0, SearchBarView.Height + SearchBarView.Margin.Top + SearchBarView.Margin.Bottom, 0, 0);

            if (width > height)
            {
                PackagesCollectionView.ItemsLayout = Device.Idiom == TargetIdiom.Tablet
                    ? new GridItemsLayout(4, ItemsLayoutOrientation.Vertical)
                    : new GridItemsLayout(2, ItemsLayoutOrientation.Vertical);
            }
            else
            {
                PackagesCollectionView.ItemsLayout = Device.Idiom == TargetIdiom.Tablet
                    ? new GridItemsLayout(2, ItemsLayoutOrientation.Vertical)
                    : new GridItemsLayout(1, ItemsLayoutOrientation.Vertical);
            }
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
            var top = e.VerticalOffset < 1;

            var transY = Convert.ToInt32(SearchBarView.TranslationY);
            if (transY == 0 &&
                e.VerticalDelta > 15 &&
                !top)
            {
                var trans = SearchBarView.Height + SearchBarView.Margin.Top;
                var safeInsets = On<Xamarin.Forms.PlatformConfiguration.iOS>().SafeAreaInsets();

                Task.WhenAll(
                    SearchBarView.TranslateTo(0, -(trans + safeInsets.Top), 250, Easing.CubicIn),
                    SearchBarView.FadeTo(0.25, 200));
            }
            else if (transY != 0 &&
                     (e.VerticalDelta < 0 && (Math.Abs(e.VerticalDelta) > 10) || top))
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
                    var packagePage = new PackagePage
                    {
                        PackageId = metadata.Id,
                        Version = metadata.Version
                    };

                    return packagePage;
                });
            }
        }
    }
}