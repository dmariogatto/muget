using System;
using System.Threading.Tasks;

namespace MuGet.Services
{
    public interface IEnvironmentService
    {
        bool NativeDarkMode { get; }

        Theme GetOperatingSystemTheme();
    }
}