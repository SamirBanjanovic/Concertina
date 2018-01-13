using System;
using System.Collections.Generic;
using System.Text;
using Concertina.Core;

namespace Concertina
{
    public sealed class ConcertinaPlugin
    { 
        public IPlugin Plugin { get; set; }

        public IPluginDetails Details { get; set; }
    }
}
