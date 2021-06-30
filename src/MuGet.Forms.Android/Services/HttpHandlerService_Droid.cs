using Android.Runtime;
using MuGet.Services;
using System.Net;
using System.Net.Http;
using Xamarin.Android.Net;

namespace MuGet.Forms.Android.Services
{
    [Preserve(AllMembers = true)]
    public class HttpHandlerService_Droid : IHttpHandlerService
    {
        private readonly ILogger _logger;

        public HttpHandlerService_Droid(ILogger logger)
        {
            _logger = logger;
        }

        public HttpMessageHandler GetNativeHandler()
            => new AndroidClientHandler()
            {
                // https://github.com/xamarin/xamarin-android/issues/2619
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
    }
}