using NuGet.Versioning;
using System;

namespace MuGet.Forms.Models
{
    public class PackageVersion
    {
        private NuGetVersion _nugetVersion;
        private Version _version;

        public PackageVersion(string version)
        {
            Original = version;

            if (!string.IsNullOrEmpty(Original))
            {
                try
                {
                    _nugetVersion = NuGetVersion.Parse(Original);
                    Version.TryParse(Original, out _version);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }                
            }
        }

        public bool IsPrerelease => _nugetVersion?.IsPrerelease ?? false;

        public string Original { get; }
        
        public override string ToString() => _nugetVersion?.ToString() ?? _version?.ToString() ?? Original;
    }
}
