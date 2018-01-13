using System;
using System.Collections.Generic;
using System.Text;

namespace Integration.Core.Entities
{
    public class FileDirectoryWatcher
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Directory { get; set; }

        public string FileFilter { get; set; }

        public bool IsRemoteLocation { get; set; }
    }
}
