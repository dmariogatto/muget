using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MuGet.Forms.Services
{
    public interface ILogger
    {
        void Error(Exception ex, IDictionary<string, string> data = null);
        void Event(string eventName, IDictionary<string, string> properties = null);

        long LogInBytes();
        Task<string> GetLog();
        void DeleteLog();
    }
}
