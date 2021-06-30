using System;
using System.Globalization;

namespace MuGet.Services
{
    public interface ILocalise
    {
        CultureInfo GetCurrentCultureInfo();
        void SetLocale(CultureInfo ci);

        bool Is24Hour { get; }
    }

}