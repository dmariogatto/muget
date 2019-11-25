using MuGet.Forms.ViewModels;
using Xamarin.Forms;
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
            NuGetSkeletonView.Margin =
            SearchCollectionHeaderView.Margin =
                new Thickness(0, SearchBarView.Height + SearchBarView.Margin.Top + 4, 0, 0);
        }

        private void ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            if (ViewModel.RemainingItemsThreshold > 0 &&
                e.ItemIndex >= ViewModel.Packages.Count - ViewModel.RemainingItemsThreshold)
            {
                ViewModel.RemainingItemsThresholdReachedCommand.ExecuteAsync();
            }
        }
    }
}