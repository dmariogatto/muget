using MuGet.Forms.Models;
using System.Collections.Generic;

namespace MuGet.Forms.Services
{
    public interface IMuGetPackageService
    {
        IList<MuGetPackage> GetPackages();
    }
}
