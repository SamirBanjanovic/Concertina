using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryWatcherPlugin.Models
{
    public class ReceivedFileInformationApiModel
    {
        public int DirectoryWatcherId { get; set; }

        public string Directory { get; set; }

        public string FileFilterUsed { get; set; }

        public string FileName { get; set; }

        public string FileExtension { get; set; }

        public string FullPath { get; set; }

        public string CorrelationId { get; set; }

        public DateTime MessageDateTime { get; set; }
    }
}
