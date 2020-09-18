using System;
using System.Globalization;
using Xamarin.Forms;

namespace MuGet.Forms.UI.Converters
{
    public class StateSquidConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var vmState = value as ViewModels.State? ?? ViewModels.State.None;

            return vmState switch
            {
                ViewModels.State.None => Xamarin.Forms.StateSquid.State.None,
                ViewModels.State.Loading => Xamarin.Forms.StateSquid.State.Loading,
                ViewModels.State.Saving => Xamarin.Forms.StateSquid.State.Saving,
                ViewModels.State.Success => Xamarin.Forms.StateSquid.State.Success,
                ViewModels.State.Error => Xamarin.Forms.StateSquid.State.Error,
                ViewModels.State.Empty => Xamarin.Forms.StateSquid.State.Empty,
                ViewModels.State.Custom => Xamarin.Forms.StateSquid.State.Custom,
                _ => Xamarin.Forms.StateSquid.State.None,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
