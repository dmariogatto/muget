using System;
using System.Drawing;

namespace MuGet.Services
{
    public interface IThemeService
    {
        Theme Current { get; }
        
        void SetTheme(Theme theme);

        Color PrimaryColor { get; }
        Color PrimaryDarkColor{ get; }
        Color PrimaryAccentColor{ get; }
    }
}
