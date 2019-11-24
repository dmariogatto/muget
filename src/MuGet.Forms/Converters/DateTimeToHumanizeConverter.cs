using System;
using System.Globalization;
using Xamarin.Forms;
using Humanizer;

namespace MuGet.Forms.Converters
{
    public class DateTimeToHumanizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
            {
                return dt > DateTime.UtcNow.AddMonths(-11) ? dt.Humanize() : dt.ToShortDateString();
            }

            return string.Empty;
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
