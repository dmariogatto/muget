using MuGet.Localisation;
using MuGet.Services;
using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuGet.Forms.UI
{
    [ContentProperty(nameof(Text))]
    public class TranslateExtension : IMarkupExtension
    {
        private const string ResourceId = "MuGet.Localisation.Resources";
        private static readonly Lazy<ResourceManager> ResMgr = new Lazy<ResourceManager>(() =>
            new ResourceManager(ResourceId, typeof(Resources).GetTypeInfo().Assembly));

        private readonly CultureInfo _ci;

        public TranslateExtension()
        {
            _ci = IoC.Resolve<ILocalise>().GetCurrentCultureInfo();
        }

        public string Text { get; set; }

        public bool AllCaps { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(Text))
                return string.Empty;

            var translation = ResMgr.Value.GetString(Text, _ci);

            if (translation == null)
            {
#if DEBUG

                throw new ArgumentException(
                    $"Key '{Text}' was not found in resources '{ResourceId}' for culture '{_ci.Name}'.",
                    "Text");
#else
                translation = Text; // returns the key, which GETS DISPLAYED TO THE USER
#endif
            }

            if (AllCaps)
            {
                translation = translation.ToUpper();
            }

            return translation;
        }
    }
}