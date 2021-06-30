using MvvmHelpers;

namespace MuGet.Models
{
    public class MuGetPackage : ObservableObject
    {
        public MuGetPackage(string packageId)
        {
            PackageId = packageId;
        }

        public string PackageId { get; set; }

        private PackageMetadata _metadata;
        public PackageMetadata Metadata
        {
            get => _metadata;
            set => SetProperty(ref _metadata, value);
        }
    }
}