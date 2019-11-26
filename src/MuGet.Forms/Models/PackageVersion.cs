using NuGet.Versioning;
using System;

namespace MuGet.Forms.Models
{
    public class PackageVersion
    {
        private SemanticVersion _semVersion;
        private Version _version;

        public PackageVersion(string version)
        {
            Original = version;

            if (!string.IsNullOrEmpty(Original))
            {
                SemanticVersion.TryParse(Original, out _semVersion);
                Version.TryParse(Original, out _version);
            }
        }

        public bool IsPrerelease => _semVersion?.IsPrerelease ?? Original.Contains("-");

        public string Original { get; }
        public override string ToString() => _semVersion?.ToFullString() ?? _version?.ToString() ?? Original;
    }
}
