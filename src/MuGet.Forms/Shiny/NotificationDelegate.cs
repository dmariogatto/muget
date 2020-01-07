using MuGet.Forms.Services;
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

        public async Task OnEntry(Notification notification)
        {
            if (!string.IsNullOrEmpty(notification?.Payload))
            {
                try
                {
                    await ((Shell)Application.Current.MainPage).GoToAsync(notification.Payload);
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
