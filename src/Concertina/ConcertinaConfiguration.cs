using System;
using System.Collections.Generic;
using System.Text;
using Concertina.Core;

namespace Concertina
{
    public class ConcertinaConfiguration
        : IConcertinaConfiguration
    {
        public string InstanceName => throw new NotImplementedException();

        public string PluginRootDirectory { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
