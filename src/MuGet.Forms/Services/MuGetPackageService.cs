using MuGet.Forms.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MuGet.Forms.Services
{
    public class MuGetPackageService : IMuGetPackageService
    {
        private readonly IList<string> _packageIds = new List<string>()
        {
            "Humanizer",
            "LiteDB",
            "Plugin.SegmentedControl.Netstandard",
            "Polly",
            "Shiny.Core",
            "TouchView ",
            "Xamarin.Forms",
            "Xamarin.Essentials",
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
                _nuGetService.GetPackageMetadata(p.PackageId, default, true)
                    .ContinueWith((task) =>
                    {
                        if (task.IsCompleted && !task.IsFaulted && task.Result != null)
                            p.Metadata = task.Result;
                    });
            }

            return _packages;
        }
    }
}
