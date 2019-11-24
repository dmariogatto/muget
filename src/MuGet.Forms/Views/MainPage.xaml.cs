using MuGet.Forms.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

namespace MuGet.Forms.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : BasePage<MainViewModel>
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            SearchBarView.TranslateTo(0, 0, 350, Easing.CubicOut);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            
            HomeView.Margin =
            NuGetSkeletonView.Margin =
            SearchCollectionHeaderView.Margin =
                new Thickness(0, SearchBarView.Height + SearchBarView.Margin.Top + 4, 0, 0);
        }

        private void PackagesScrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            if (e.VerticalDelta > 15)
            {
                var trans = SearchBarView.Height + SearchBarView.Margin.Top;
                var safeInsets = On<Xamarin.Forms.PlatformConfiguration.iOS>().SafeAreaInsets();
                SearchBarView.TranslateTo(0, -(trans + safeInsets.Top), 250, Easing.CubicIn);
            }
            else if (e.VerticalDelta < 0 &&
                     Math.Abs(e.VerticalDelta) > 10)
            {
                SearchBarView.TranslateTo(0, 0, 250, Easing.CubicOut);
            }
        }
    }
}