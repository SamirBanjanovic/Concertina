using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Concertina.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;

namespace DirectoryWatcherPlugin
{
    public class DirectoryWatcherManager
        : IPlugin
    {        
        private readonly IDictionary<int, (DirectoryWatcherDetailModel, FileSystemWatcher)> _fileSystemWatchers;

        public DirectoryWatcherManager()            
        {
            _fileSystemWatchers = new Dictionary<int, (DirectoryWatcherDetailModel, FileSystemWatcher)>();          
        }

        public string Name { get { return nameof(DirectoryWatcherManager); } }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Start(int resourceId)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Stop(int resourceId)
        {
            throw new NotImplementedException();
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }

        private void CreateWatchers(IEnumerable<DirectoryWatcherDetailModel> watcherDetails)
        {
            foreach (var detail in watcherDetails)
            {

            }
        }

        private bool TryCreateWatcher(DirectoryWatcherDetailModel details, FileSystemWatcher watcher)
        {
            try
            {
                watcher = null;
                if (IsFileFilterValidForDirectory(details.Directory, details.FileFilter))
                {
                    watcher = new FileSystemWatcher(details.Directory, details.FileFilter);
                    watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
                    watcher.Created += (e, s) => SendFileCreatedNotification(details, s);
                    watcher.EnableRaisingEvents = true;
                    return true;
                }
            }
            catch (Exception e)
            {
                //_logger.LogError(e, $"Failed to create Watcher for \"{details.Directory}\" using filter \"{details.FileFilter}\"");
            }

            return false;
        }

        private void SendFileCreatedNotification(DirectoryWatcherDetailModel details, FileSystemEventArgs args)
        {
            var messageDetails = new ReceivedFileInformation
            {
                DirectWatcherId = details.Id,
                FileFilterUsed = details.FileFilter,
                FullyQualifiedFileName = args.FullPath,
                CorrelationId = Guid.NewGuid().ToString("N")               
            };
        }


        private bool IsFileFilterValidForDirectory(string directory, string fileFilter)
        {
            var directoryWatchers = _fileSystemWatchers.Values.Where(x => x.Item2.Path == directory).ToList();
            if (_fileSystemWatchers.Any())
            {
                if (fileFilter == "*.*" || fileFilter == ".*")
                {
                    return false;
                }

                if (directoryWatchers.Any(p => p.Item2.Filter == "*.*" || p.Item2.Filter == ".*"))
                {
                    return false;
                }

                if (directoryWatchers.Any(p => p.Item2.Filter.Equals(fileFilter, StringComparison.CurrentCultureIgnoreCase)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
