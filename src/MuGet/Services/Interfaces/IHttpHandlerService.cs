using System.Net.Http;

namespace MuGet.Services
{
    public interface IHttpHandlerService
    {
        HttpMessageHandler GetNativeHandler();
    }
}
