using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Concertina
{
    public class AssemblyPlugin
    {
        public Assembly Assembly { get; set; }

        public IEnumerable<TypeInfo> PluginTypes { get; set; }
    }
}
