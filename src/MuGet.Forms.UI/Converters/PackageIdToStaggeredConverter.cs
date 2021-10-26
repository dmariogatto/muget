using System;
using System.Globalization;
using Xamarin.Forms;

namespace MuGet.Forms.UI.Converters
{
    public class PackageIdToStaggeredConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string id
                ? id.Replace(".", $".{Environment.NewLine}")
                : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}