using MuGet.Forms.Services;
using MuGet.Forms.Views;
using Shiny.Notifications;
using System;
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

            if (!string.IsNullOrEmpty(notification?.Payload))
            {
                try
                {
                    var payload = notification.Payload.Split(',');
                    if (payload.Length == 2 &&
                        Application.Current.MainPage is NavigationPage navPage)
                    {
                        var packageId = payload[0];
                        var version = payload[1];

                        var packagePage = new PackagePage();
                        packagePage.PackageId = packageId;
                        packagePage.Version = version;

                        await navPage.PushAsync(packagePage);
                    }
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
