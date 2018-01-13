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
                    RegisterPlugin(cp.Details.TypeInfo);
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
                var hasAssemblyFiles = TryGetAssemblyFilesFromPluginDirectory(pluginDirectory, out IEnumerable<string> assemblyFiles);

                if (hasAssemblyFiles && assemblyFiles?.Count() > 0)
                {

                    Parallel.ForEach(assemblyFiles, file =>
                    {
                        var assembly = Assembly.LoadFrom(file);
                        if (TryGetAnyPluginsFromAssembly(assembly, out TypeInfo[] pluginTypes))
                        {
                            var assemblyTasks = InitializePlugins(pluginTypes);
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
                        RegisterPlugin(pluginType);
                    }
                    catch (Exception e)
                    {
                        _logger.LogCritical(e, $"Unhandled error registering plugin {pluginType.FullName} from assembly {pluginType.Assembly.FullName}");
                    }
                });
            }

            return tasks;
        }

        private void RegisterPlugin(TypeInfo pluginType)
        {
            if (TryCreateInstance(pluginType, out IPlugin plugin))
            {
                var concertinaPlugin = new ConcertinaPlugin
                {
                    Plugin = plugin,
                    Details = plugin.SetState(PluginState.Initialized)
                };

                _concertinaPlugins.AddOrUpdate(pluginType.FullName, concertinaPlugin, (k, v) => concertinaPlugin);
            }
            else
            {
                _logger.LogInformation($"Failed to create instance of type {pluginType.FullName} from assembly {pluginType.Assembly.FullName}");
            }
        }

        private bool TryCreateInstance(TypeInfo typeInfo, out IPlugin plugin)
        {
            plugin = null;

            if (typeInfo == null)
            {
                _logger.LogWarning("Received null \"TypeInfo\"");
                return false;
            }

            try
            {
                plugin = (IPlugin)typeInfo.Assembly.CreateInstance(typeInfo.FullName, false);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"Error creating instance of plugin \"{ typeInfo.FullName }\" in assembly \"{typeInfo.Assembly}\"");
                return false;
            }

        }

        #endregion initialization helpers


        #region Helper Methods

        private bool TryGetAssemblyFilesFromPluginDirectory(string pluginDirectory, out IEnumerable<string> assemblyFiles)
        {
            assemblyFiles = null;

            try
            {
                assemblyFiles = Directory.GetFiles(pluginDirectory, "*.dll");

                return true;
            }
            catch (Exception e)
            {
                // log
                _logger.LogCritical(e, $"Error searching for assemblies in plugin directory \"{pluginDirectory}\"");
                return false;
            }
        }

        private bool TryGetAnyPluginsFromAssembly(Assembly assembly, out TypeInfo[] pluginTypes)
        {
            pluginTypes = null;
            try
            {
                pluginTypes = assembly.DefinedTypes.Where(p => p.ImplementedInterfaces.Contains(typeof(IPlugin))).ToArray();

                return pluginTypes.Any();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"Error locating \"IPlugin\" implementations in assembly \"{assembly.FullName}\"");
                return false;
            }


        }

        #endregion Helper Methods
    }
}
