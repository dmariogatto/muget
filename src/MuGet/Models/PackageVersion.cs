using NuGet.Versioning;
using System;

namespace MuGet.Models
{
    public class PackageVersion : IComparable, IComparable<PackageVersion>
    {
        public PackageVersion(string version)
        {
            Original = version;

            if (!string.IsNullOrEmpty(Original))
            {
                try
                {
                    NuGetVersion = NuGetVersion.Parse(Original);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }                
            }
        }

        public NuGetVersion NuGetVersion { get; }
        public string Original { get; }

        public bool IsPrerelease => NuGetVersion?.IsPrerelease ?? false;
        public bool IsSemVer2 => NuGetVersion?.IsSemVer2 ?? false;

        public override string ToString() => NuGetVersion?.ToString() ?? Original;

        public int CompareTo(object obj)
        {
            return CompareTo(obj as PackageVersion);
        }

        public int CompareTo(PackageVersion other)
        {
            return NuGetVersion?.CompareTo(other?.NuGetVersion) ?? -1;
        }
    }
}
