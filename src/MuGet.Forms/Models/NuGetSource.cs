using System;
using System.Collections.Generic;
using System.Text;

namespace MuGet.Models
{
    public class NuGetSource
    {
        public string Version { get; set; }
        public List<NuGetResource> Resources { get; set; }
    }
}
