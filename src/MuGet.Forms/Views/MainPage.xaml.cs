using System;
using System.Threading.Tasks;
using MuGet.Forms.ViewModels;
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

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            HomeView.Margin =
            SearchCollectionHeaderView.Margin =
                new Thickness(0, SearchBarView.Height + SearchBarView.Margin.Top + 4, 0, 0);

            ActivityIndicatorView.Margin =
                new Thickness(0, SearchBarView.Height + SearchBarView.Margin.Top + 20, 0, 0);
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
    }
}