using System;
using System.Collections.Generic;
using System.Text;

namespace Concertina.Core
{
    public enum PluginState
    {
        Initialized = 0,
        Started,
        Stopped,
        Errored,
        Terminated
    }
}
