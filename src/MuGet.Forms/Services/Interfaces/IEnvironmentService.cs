using System;
using System.Threading.Tasks;

namespace MuGet.Forms.Services
{
    public interface IEnvironmentService
    {
        bool NativeDarkMode { get; }

        Theme GetOperatingSystemTheme();        
    }
}
