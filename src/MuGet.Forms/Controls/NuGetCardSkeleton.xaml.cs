using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuGet.Forms.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NuGetCardSkeleton : ContentView
    {
        public NuGetCardSkeleton()
        {
            InitializeComponent();
        }
    }
}