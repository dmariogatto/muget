using Foundation;
using System.Net.Http;

namespace MuGet.Services
{
    [Preserve(AllMembers = true)]
    public class HttpHandlerService_iOS : IHttpHandlerService
    {
        private readonly ILogger _logger;

        public HttpHandlerService_iOS(ILogger logger)
        {
            _logger = logger;
        }

        public HttpMessageHandler GetNativeHandler()
            => new NSUrlSessionHandler();
    }
}