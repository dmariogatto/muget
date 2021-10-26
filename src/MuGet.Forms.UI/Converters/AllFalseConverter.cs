using System;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;

namespace MuGet.Forms.UI.Converters
{
    public class AllFalseConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
                return false;

            return values.All(i => i is bool b && !b);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}