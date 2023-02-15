using Android.Runtime;
using Cats.CertificateTransparency;
using Cats.CertificateTransparency.Models;
using MuGet.Services;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

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
            => new CatsAndroidMessageHandler(VerifyCtResult)
            {
                // https://github.com/xamarin/xamarin-android/issues/2619
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

        private bool VerifyCtResult(string host, IList<X509Certificate2> chain, CtVerificationResult result)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"😺 CT Result, host: {host}, description: {result?.Description ?? string.Empty}");
#endif

            if (!result.IsValid)
            {
                _logger.Event("ct_result_invalid", new Dictionary<string, string>()
                {
                    { "host", host },
                    { "result", result?.Result.ToString() ?? string.Empty },
                    { "description", result?.Description ?? string.Empty }
                });
            }

            // Sunset time, all is well
            return true;
        }
    }
}