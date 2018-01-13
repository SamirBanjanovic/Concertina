using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Concertina.Core;
using Microsoft.Extensions.Logging;
//using NLog;
using NLog.Extensions.Logging;

namespace Concertina
{
    public class ConcertinaService
        : IConcertinaService
        , IHostConcertina
    {     
        private readonly ILogger _logger;
        private readonly IPluginManager _pluginManager;

        public ConcertinaService(IConcertinaConfiguration configuration, IPluginManager pluginManager, ILoggerFactory loggerFactory)
        {
            ServiceConfiguration = configuration;
            _pluginManager = pluginManager;            
            _logger = loggerFactory.CreateLogger<ConcertinaService>();
        }

        public IConcertinaConfiguration ServiceConfiguration { get; private set; }

        public void Start()
        {
            _pluginManager.Start();
        }

        public void Stop()
        {
            
        }

        public void Start(string pluginName)
        {
            
        }

        public void Stop(string pluginName)
        {
            
        }

        public void Start(string pluginName, int resourceId)
        {
            
        }

        public void Stop(string pluginName, int resourceId)
        {
            
        }
    }
}
