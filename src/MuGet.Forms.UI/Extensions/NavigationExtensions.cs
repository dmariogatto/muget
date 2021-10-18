using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MuGet.Forms.UI.Extentions
{
    public static class NavigationExtensions
    {
        private static bool IsNavigating;

        public static async Task PushPageFactoryAsync(this INavigation navigation, Func<Page> pageFunc, bool animated = true)
        {
            if (IsNavigating)
                return;

            IsNavigating = true;

            await navigation.PushAsync(pageFunc(), animated);

            IsNavigating = false;
        }
    }
}
