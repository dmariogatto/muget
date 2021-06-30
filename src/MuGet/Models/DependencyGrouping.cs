using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuGet.Models
{
    public class DependencyGrouping : Grouping<string, Dependency>
    {
        public DependencyGrouping(string key, IEnumerable<Dependency> dependencies)
            : base(key, dependencies)
        { }
    }
}