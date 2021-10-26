using System;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;

namespace MuGet.Forms.UI.Converters
{
    public class AllEqualConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Any(i => i == null))
                return false;

            if (values.Length == 0)
                return true;

            var first = values.First();
            return values.Skip(1).All(i => i.Equals(first));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
