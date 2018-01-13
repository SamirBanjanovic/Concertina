using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryWatcherPlugin
{
    public class DirectoryWatcherDetailModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Directory { get; set; }

        public string FileFilter { get; set; }

        public bool IsRemoteLocataion { get; set; }
    }
}
