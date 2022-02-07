using MuGet.Forms.UI.Extentions;
using MuGet.Models;
using MuGet.ViewModels;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace MuGet.Forms.UI.Views
{
    public partial class MainPage : BasePage<MainViewModel>
    {
        private IDeviceDisplay _deviceDisplay;

        public MainPage() : base()
        {
            InitializeComponent();

            _deviceDisplay = IoC.Resolve<IDeviceDisplay>();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            UpdateDisplayInfo(_deviceDisplay.MainDisplayInfo);
            _deviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;

            Task.WhenAll(
                SearchBarView.TranslateTo(0, 0, 250, Easing.CubicOut),
                SearchBarView.FadeTo(1, 200));
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _deviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (width > 0 && height > 0)
            {
                SearchBarView.WidthRequest = Device.Idiom == TargetIdiom.Tablet
                    ? width / 1.8d
                    : width / 1.4d;

                HomeView.Padding =
                    PackagesSkeletonView.Padding =
                        PackagesCollectionViewHeader.Padding =
                            new Thickness(0, SearchBarView.Height + SearchBarView.Margin.Top + SearchBarView.Margin.Bottom, 0, 0);
            }
        }

        private void OnMainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
        {
            UpdateDisplayInfo(e.DisplayInfo);
        }

        private void UpdateDisplayInfo(DisplayInfo displayInfo)
        {
            if (displayInfo.Orientation == DisplayOrientation.Landscape)
            {
                PackagesCollectionView.ItemsLayout = Device.Idiom == TargetIdiom.Tablet
                    ? new GridItemsLayout(2, ItemsLayoutOrientation.Vertical)
                    : new GridItemsLayout(1, ItemsLayoutOrientation.Vertical);
            }
            else
            {
                PackagesCollectionView.ItemsLayout = Device.Idiom == TargetIdiom.Tablet
                    ? new GridItemsLayout(1, ItemsLayoutOrientation.Vertical)
                    : new GridItemsLayout(1, ItemsLayoutOrientation.Vertical);
            }

            SearchBarView.CancelAnimations();
            SearchBarView.TranslationY = 0;
            SearchBarView.Opacity = 1;
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