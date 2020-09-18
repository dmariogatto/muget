using Android.Runtime;
using Android.Text.Format;
using MuGet.Models;
using MuGet.Services;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace MuGet.Forms.Services
{
    [Preserve(AllMembers = true)]
    public class LocaliseService_Droid : ILocalise
    {
        private readonly IDictionary<string, CultureInfo> _cultures = new Dictionary<string, CultureInfo>();

        public void SetLocale(CultureInfo ci)
        {
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }

        public CultureInfo GetCurrentCultureInfo()
        {
            var netLanguage = AndroidToDotnetLanguage(Java.Util.Locale.Default.ToString().Replace("_", "-"));
           
            if (!_cultures.ContainsKey(netLanguage))
            {
                // this gets called a lot - try/catch can be expensive so consider caching or something
                CultureInfo ci = null;
                try
                {
                    ci = new CultureInfo(netLanguage);
                }
                catch (CultureNotFoundException e1)
                {
                    // locale not valid .NET culture (eg. "en-ES" : English in Spain)
                    // fallback to first characters, in this case "en"
                    try
                    {
                        var fallback = ToDotnetFallbackLanguage(new PlatformCulture(netLanguage));
                        ci = new CultureInfo(fallback);
                    }
                    catch (CultureNotFoundException e2)
                    {
                        // language not valid .NET culture, falling back to English
                        ci = new CultureInfo("en");
                    }
                }

                if (ci != null)
                    _cultures.Add(netLanguage, ci);
            }

            return _cultures[netLanguage];
        }

        public bool Is24Hour
            => DateFormat.Is24HourFormat(Xamarin.Essentials.Platform.AppContext);

        private string AndroidToDotnetLanguage(string androidLanguage)
        {
            var netLanguage = androidLanguage;
            //certain languages need to be converted to CultureInfo equivalent
            switch (androidLanguage)
            {
                case "ms-BN":   // "Malaysian (Brunei)" not supported .NET culture
                case "ms-MY":   // "Malaysian (Malaysia)" not supported .NET culture
                case "ms-SG":   // "Malaysian (Singapore)" not supported .NET culture
                    netLanguage = "ms"; // closest supported
                    break;
                case "in-ID":  // "Indonesian (Indonesia)" has different code in  .NET
                    netLanguage = "id-ID"; // correct code for .NET
                    break;
                case "gsw-CH":  // "Schwiizertüütsch (Swiss German)" not supported .NET culture
                    netLanguage = "de-CH"; // closest supported
                    break;
                    // add more application-specific cases here (if required)
                    // ONLY use cultures that have been tested and known to work
            }
            return netLanguage;
        }

        private string ToDotnetFallbackLanguage(PlatformCulture platCulture)
        {
            var netLanguage = platCulture.LanguageCode; // use the first part of the identifier (two chars, usually);
            switch (platCulture.LanguageCode)
            {
                case "gsw":
                    netLanguage = "de-CH"; // equivalent to German (Switzerland) for this app
                    break;
                    // add more application-specific cases here (if required)
                    // ONLY use cultures that have been tested and known to work
            }
            return netLanguage;
        }
    }
}