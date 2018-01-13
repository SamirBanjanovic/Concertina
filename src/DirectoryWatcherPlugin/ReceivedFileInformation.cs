using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryWatcherPlugin
{
    public class ReceivedFileInformation
    {
        public int DirectWatcherId { get; set; }

        public string FileFilterUsed { get; set; }

        public string FullyQualifiedFileName { get; set; }
        
        public string CorrelationId { get; set; }
    }
}
