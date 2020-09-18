using MuGet.Models;
using System.Collections.Generic;

namespace MuGet.Services
{
    public interface IMuGetPackageService
    {
        IList<MuGetPackage> GetPackages();
    }
}
