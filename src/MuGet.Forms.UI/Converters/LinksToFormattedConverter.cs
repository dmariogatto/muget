using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MuGet.Forms.UI.Converters
{
    public class LinksToFormattedConverter : IValueConverter
    {
        private static readonly Regex UrlPattern = new Regex(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)", RegexOptions.Compiled);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var formattedString = new FormattedString();

            if (value is string text && !string.IsNullOrEmpty(text))
            {
                var matches = UrlPattern.Matches(text);

                if (matches.Count > 0)
                {
                    var previousIdx = 0;
                    foreach (Match m in matches)
                    {
                        var substring = text.Substring(previousIdx, m.Index - previousIdx);
                        previousIdx += substring.Length;
                        if (substring.Length > 0)
                            formattedString.Spans.Add(new Span() { Text = substring });                        
                        
                        substring = text.Substring(previousIdx, m.Length);
                        previousIdx += substring.Length;
                        if (substring.Length > 0)
                        {
                            var urlSpan = new Span()
                            { 
                                Text = substring,
                                TextColor = (Color)Application.Current.Resources["LinkColor"]
                            };
                            var tappedGesture = new TapGestureRecognizer();
                            tappedGesture.Tapped += delegate { Browser.OpenAsync(substring); };
                            urlSpan.GestureRecognizers.Add(tappedGesture);
                            formattedString.Spans.Add(urlSpan);
                        }
                    }

                    var final = text.Substring(previousIdx, text.Length - previousIdx);
                    if (final?.Length > 0)
                        formattedString.Spans.Add(new Span() { Text = final });
                }
                else
                {
                    formattedString.Spans.Add(new Span() { Text = text });
                }
            }

            return formattedString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
