using Acr.UserDialogs;
using MuGet.Services;

namespace MuGet.ViewModels
{
    public class BvmConstructor : IBvmConstructor
    {
        public INuGetService NuGetService { get; }
        public ILogger Logger { get; }
        public IUserDialogs UserDialogs { get; }

        public BvmConstructor(
            INuGetService nuGetService,
            ILogger logger,
            IUserDialogs userDialogs) : base()
        {
            NuGetService = nuGetService;
            Logger = logger;
            UserDialogs = userDialogs;
        }
    }
}
