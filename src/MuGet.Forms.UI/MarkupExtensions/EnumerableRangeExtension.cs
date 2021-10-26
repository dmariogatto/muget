using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuGet.Forms.UI
{
    [ContentProperty(nameof(Count))]
    public class EnumerableRangeExtension : IMarkupExtension
    {
        public EnumerableRangeExtension()
        {
        }

        public int Count { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Count < 1
                ? Enumerable.Empty<int>()
                : Enumerable.Range(0, Count);
        }
    }
}