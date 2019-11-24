using System;
using System.Globalization;
using Xamarin.Forms;

namespace MuGet.Forms.Converters
{
    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var offOpacity = parameter as double? ?? 0d;
            return value as bool? ?? false
                ? 1d
                : offOpacity;
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
