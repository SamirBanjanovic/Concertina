using System;
using System.Collections.Generic;
using System.Text;

namespace Integration.Core
{
    public enum JobRequestSource
    {
        DirectoryWatcher = 1,
        TimedScheduler = 2,
        HttpPost = 3,
        UDP,
        TCPIP,
    }
}
