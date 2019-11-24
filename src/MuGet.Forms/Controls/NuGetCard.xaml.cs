using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuGet.Forms.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NuGetCard : ContentView
    {
        public NuGetCard()
        {
            InitializeComponent();
        }
    }
}