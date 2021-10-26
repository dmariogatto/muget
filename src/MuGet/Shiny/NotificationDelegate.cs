using MuGet.Models;
using MuGet.Services;
using Shiny.Notifications;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

namespace MuGet
{
    public class NotificationDelegate : INotificationDelegate
    {
        private readonly ILauncher _launcher;
        private readonly ILogger _logger;

        public NotificationDelegate(ILauncher launcher, ILogger logger)
        {
            _launcher = launcher;
            _logger = logger;
        }

        public async Task OnEntry(NotificationResponse response)
        {
            var notification = response.Notification;
            var payload = notification?.Payload;

            if (payload?.Any() == true &&
                payload.ContainsKey(nameof(CatalogEntry.Id)) &&
                payload.ContainsKey(nameof(CatalogEntry.Version)))
            {
                try
                {
                    var packageId = payload[nameof(CatalogEntry.Id)];
                    var version = payload[nameof(CatalogEntry.Version)];

                    await _launcher.TryOpenAsync($"muget://package/{packageId}/{version}");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }

        public Task OnReceived(Notification notification)
        {
            return Task.CompletedTask;
        }
    }
}