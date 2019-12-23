using NuGet.Versioning;
using System;

namespace MuGet.Forms.Models
{
    public class PackageVersion
    {
        private NuGetVersion _semVersion;
        private Version _version;

        public PackageVersion(string version)
        {
            Original = version;

            if (!string.IsNullOrEmpty(Original))
            {
                try
                {
                    _semVersion = NuGetVersion.Parse(Original);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
                Version.TryParse(Original, out _version);
            }
        }

        public bool IsPrerelease
        {
            get
            {
                var isPre = false;

                if (_semVersion != null)
                {
                    isPre = _semVersion.IsPrerelease;
                }
                else
                {
                    var plusIdx = Original.IndexOf('+');
                    var dashIdx = Original.IndexOf('-');
                    isPre = dashIdx > 0 && (plusIdx < 0 || dashIdx < plusIdx);
                }

                return isPre;                
            }
        }

        public string Original { get; }
        public override string ToString() => _semVersion?.ToFullString() ?? _version?.ToString() ?? Original;
    }
}
