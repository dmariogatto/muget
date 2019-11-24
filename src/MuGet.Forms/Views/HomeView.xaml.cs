using MuGet.Forms.ViewModels;
using Xamarin.Forms.Xaml;

namespace MuGet.Forms.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomeView : BaseView<HomeViewModel>
    {
        public HomeView()
        {
            InitializeComponent();
        }
    }
}