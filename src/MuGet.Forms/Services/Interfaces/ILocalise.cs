using System;
using System.Globalization;

namespace MuGet.Forms.Services
{
    public interface ILocalise
    {
        CultureInfo GetCurrentCultureInfo();
        void SetLocale(CultureInfo ci);

        bool Is24Hour { get; }
    }

}
