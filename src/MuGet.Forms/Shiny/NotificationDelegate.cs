using MuGet.Forms.Models;
using MuGet.Forms.Services;
using MuGet.Forms.Views;
using Shiny.Notifications;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MuGet.Forms
{
    public class NotificationDelegate : INotificationDelegate
    {
        private readonly ILogger _logger;

        public NotificationDelegate(ILogger logger)
        {
            _logger = logger;
        }

        public async Task OnEntry(NotificationResponse response)
        {
            var notification = response.Notification;
            var payload = notification?.Payload;

            if (payload?.Any() == true &&
                payload.ContainsKey(nameof(CatalogEntry.Id)) &&
                payload.ContainsKey(nameof(CatalogEntry.Version)) &&
                Application.Current.MainPage is NavigationPage navPage)
            {
                try
                {
                    var packageId = payload[nameof(CatalogEntry.Id)];
                    var version = payload[nameof(CatalogEntry.Version)];

                    var packagePage = new PackagePage
                    {
                        PackageId = packageId,
                        Version = version
                    };

                    await navPage.PushAsync(packagePage);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }

        public Task OnReceived(Notification notification)
        {
            return Task.FromResult(0);
        }
    }
}
