using Microsoft.Extensions.DependencyInjection;
using MuGet.Services;
using Shiny;
using Shiny.Jobs;
using Xamarin.Essentials.Interfaces;

namespace MuGet
{
    public class ShinyStartup : Shiny.ShinyStartup
    {
        public readonly static JobInfo NuGetJobInfo = new JobInfo(typeof(NuGetJob), nameof(NuGetJob))
        {
            BatteryNotLow = true,
            DeviceCharging = false,
            RequiredInternetAccess = InternetAccess.Any,
            Repeat = true
        };

        public override void ConfigureServices(IServiceCollection services)
        {
            Shiny.Notifications.AndroidOptions.DefaultLaunchActivityFlags = Shiny.Notifications.AndroidActivityFlags.NewTask;

            // register your shiny services here
            services.UseNotifications<NotificationDelegate>(false);

            services.AddSingleton(typeof(ILauncher), (p) => IoC.Resolve<ILauncher>());
            services.AddSingleton(typeof(INuGetService), (p) => IoC.Resolve<INuGetService>());
            services.AddSingleton(typeof(ILogger), (p) => IoC.Resolve<ILogger>());
        }
    }
}