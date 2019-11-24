using System;
using System.Collections.Generic;
using System.Text;

namespace MuGet.Forms.Exceptions
{
    public class ApiException : Exception
    {
        public int StatusCode { get; set; }

        public string Content { get; set; }
    }
}
