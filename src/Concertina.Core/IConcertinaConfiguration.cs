using System;
using System.Collections.Generic;
using System.Text;

namespace Concertina.Core
{
    public interface IConcertinaConfiguration
    {
        string InstanceName { get; }

        string PluginRootDirectory { get; set; }
    }
}
