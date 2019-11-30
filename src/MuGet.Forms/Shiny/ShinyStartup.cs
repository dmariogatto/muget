using Microsoft.Extensions.DependencyInjection;
using MuGet.Forms.Services;
using Shiny;
using Shiny.Jobs;

namespace MuGet.Forms
{
    public class ShinyStartup : Shiny.ShinyStartup
    {
        public readonly static JobInfo NuGetJobInfo = new JobInfo
        {
            Identifier = nameof(NuGetJob),
            Type = typeof(NuGetJob),
            BatteryNotLow = true,
            DeviceCharging = false,
            RequiredInternetAccess = InternetAccess.Any,
            Repeat = true
        };

        public override void ConfigureServices(IServiceCollection services)
        {
            // register your shiny services here
            services.UseNotifications<NotificationDelegate>();

            services.AddSingleton<ILogger, Logger>();
            services.AddSingleton<ICacheProvider, InMemoryCache>();
            services.AddSingleton<INuGetService, NuGetService>();            
        }
        
    }
}
