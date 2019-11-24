using MuGet.Forms.Helpers;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace MuGet.Forms.Converters
{
    public class BoolToHeartConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            return value as bool? ?? false
                ? MaterialFont.Heart
                : MaterialFont.HeartOutline;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
