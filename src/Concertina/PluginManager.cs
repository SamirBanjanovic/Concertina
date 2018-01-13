using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Concertina.Core;
using Microsoft.Extensions.Logging;

namespace Concertina
{
    public sealed class PluginManager
        : IPluginManager
    {
        private readonly ILogger<PluginManager> _logger;
        private readonly IPluginManagerConfiguration _pluginManagerConfiguration;
        private readonly IDictionary<string, string> _pluginLocations;
        private readonly ConcurrentDictionary<string, ConcertinaPlugin> _concertinaPlugins;

        public PluginManager(ILoggerFactory loggerFactory, IPluginManagerConfiguration pluginManagerConfiguration)
        {
            _logger = loggerFactory.CreateLogger<PluginManager>();
            _pluginManagerConfiguration = pluginManagerConfiguration;
            _pluginLocations = new Dictionary<string, string>();
            _concertinaPlugins = new ConcurrentDictionary<string, ConcertinaPlugin>();
        }

        public IEnumerable<(string, Stack<IPluginDetails>)> GetHistory() => ConcertinaPluginStateHistory.GetHistory();

        public IEnumerable<string> GetErroredPlugins() => _concertinaPlugins.Values.Where(x => x.Details.State == PluginState.Errored).Select(x => x.Details.Name);

        public bool HasErroredPlugins => _concertinaPlugins.Where(x => x.Value.Details.State == PluginState.Errored).Any();

        public void Start()
        {
            Parallel.ForEach(_concertinaPlugins.Values, cp =>
            {
                InternalStartPlugin(cp);
            });
        }

        public void Stop()
        {
            Parallel.ForEach(_concertinaPlugins.Values, cp =>
            {
                InternalStopPlugin(cp);
            });
        }

        public void Start(string pluginName)
        {
            if (_concertinaPlugins.TryGetValue(pluginName, out ConcertinaPlugin cp))
            {
                InternalStartPlugin(cp);
            }
        }

        public void Stop(string pluginName)
        {
            if (_concertinaPlugins.TryGetValue(pluginName, out ConcertinaPlugin cp))
            {
                InternalStopPlugin(cp);
            }
        }

        public void ReInitialize(string pluginName)
        {
            // find plugin in dictionary
            // clear instance
            // reset everything
            // keep history
            if (_concertinaPlugins.TryGetValue(pluginName, out ConcertinaPlugin cp))
            {
                try
                {
                    CreateInstanceAndRegisterPlugin(cp.Details.TypeInfo);
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, $"Error trying to re-initialize plugin {cp.Details.Name} from assembly {cp.Details.TypeInfo.Assembly.FullName}");
                }
            }
        }

        private void InternalStartPlugin(ConcertinaPlugin cp)
        {
            try
            {
                cp.Plugin.Start();
                cp.Details = cp.Plugin.SetState(PluginState.Started);
                _logger.LogInformation($"Started plugin {cp.Details.Name}");
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"Error trying to start plugin {cp.Details.Name}");
                cp.Details = cp.Plugin.SetState(PluginState.Errored);
            }
        }

        private void InternalStopPlugin(ConcertinaPlugin cp)
        {
            try
            {
                cp.Plugin.Start();
                cp.Details = cp.Plugin.SetState(PluginState.Stopped);
                _logger.LogInformation($"Stoped plugin {cp.Details.Name}");
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"Error trying to stop plugin {cp.Details.Name}");
                cp.Details = cp.Plugin.SetState(PluginState.Errored);
            }
        }

        #region initialization helpers

        private void LoadPlugins()
        {
            var pluginDirectories = Directory.GetDirectories(_pluginManagerConfiguration.PluginDirectoryRoot);
            var taskBag = new ConcurrentBag<Task>();

            foreach (var pluginDirectory in pluginDirectories)
            {
                var assemblyFiles = Directory.GetFiles(pluginDirectory, "*.dll");
                if (assemblyFiles?.Count() > 0)
                {
                    Parallel.ForEach(assemblyFiles, file =>
                    {                        
                        var assemblyPlugins = Assembly.LoadFrom(file)
                                                      .DefinedTypes
                                                      .Where(p => p.ImplementedInterfaces.Contains(typeof(IPlugin)))
                                                      .ToArray();

                        if (assemblyPlugins?.Any() == true)
                        {
                            var assemblyTasks = InitializePlugins(assemblyPlugins);
                            foreach (var task in assemblyTasks)
                            {
                                taskBag.Add(task);
                            }
                        }
                    });
                }
            }

            Task.WaitAll(taskBag.ToArray());
        }

        private Task[] InitializePlugins(TypeInfo[] pluginTypes)
        {
            if (pluginTypes.Length < 1)
                return null;

            var tasks = new Task[pluginTypes.Length];

            for (var i = 0; i < pluginTypes.Length; i++)
            {
                var pluginType = pluginTypes[i];

                tasks[i] = Task.Run(() => 
                {
                    try
                    {
                        CreateInstanceAndRegisterPlugin(pluginType);
                    }
                    catch (Exception e)
                    {
                        _logger.LogCritical(e, $"Unhandled error registering plugin {pluginType.FullName} from assembly {pluginType.Assembly.FullName}");
                    }
                });
            }

            return tasks;
        }

        private void CreateInstanceAndRegisterPlugin(TypeInfo pluginType)
        {
            var plugin = (IPlugin)pluginType.Assembly.CreateInstance(pluginType.FullName, false);

            var concertinaPlugin = new ConcertinaPlugin
            {
                Plugin = plugin,
                Details = plugin.SetState(PluginState.Initialized)
            };

            _concertinaPlugins.AddOrUpdate(pluginType.FullName, concertinaPlugin, (k, v) => concertinaPlugin);
        }

        #endregion initialization helpers
    }
}
