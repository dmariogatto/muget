using System;

namespace MuGet.Forms.Services
{
    public interface ICacheProvider
    {
        T Get<T>(string key);
        void Set<T>(string key, T value, TimeSpan expires);
    }
}
