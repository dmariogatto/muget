using Acr.UserDialogs;
using MuGet.Services;

namespace MuGet.ViewModels
{
    public interface IBvmConstructor
    {
        INuGetService NuGetService { get; }
        ILogger Logger { get; }
        IUserDialogs UserDialogs { get; }
    }
}