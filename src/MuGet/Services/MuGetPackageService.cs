using MuGet.Models;
using System.Collections.Generic;
using System.Linq;

namespace MuGet.Services
{
    public class MuGetPackageService : IMuGetPackageService
    {
        private readonly IList<string> _packageIds = new List<string>()
        {
            "AiForms.SettingsView",
            "Humanizer",
            "LiteDB",
            "Microsoft.AppCenter.Analytics",
            "Microsoft.AppCenter.Crashes",
            "Newtonsoft.Json",
            "NuGet.Versioning",
            "Plugin.StoreReview",
            "Polly",
            "Refractored.MvvmHelpers",
            "Shiny.Core",
            "Shiny.Notifications",
            "Xamarin.Essentials",
            "Xamarin.FFImageLoading.Forms",
            "Xamarin.Forms",
            "Xamarin.Forms.PancakeView",
        };

        private readonly INuGetService _nuGetService;

        private readonly IList<MuGetPackage> _packages;

        public MuGetPackageService(INuGetService nuGetService)
        {
            _nuGetService = nuGetService;
            _packages = _packageIds.OrderBy(i => i).Select(i => new MuGetPackage(i)).ToList();
        }

        public IList<MuGetPackage> GetPackages()
        {
            foreach (var p in _packages)
            {
                if (p.Metadata == null)
                    _nuGetService.GetPackageMetadataAsync(p.PackageId, default, true)
                        .ContinueWith((task) =>
                        {
                            if (task.IsCompleted &&
                                !task.IsFaulted &&
                                task.Result != null &&
                                p.Metadata == null)
                                p.Metadata = task.Result;
                        });
            }

            return _packages;
        }
    }
}