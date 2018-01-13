using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Concertina.Core;
using Microsoft.Extensions.Logging;

namespace Concertina
{
    public class PluginManagerV1
        : IPluginManager
    {
        // add logic to disable setting of service and configuration

        private readonly ILogger<PluginManagerV1> _logger;
        private readonly IPluginManagerConfiguration _pluginManagerConfiguration;
        private readonly ConcurrentDictionary<string, (IPlugin, IPluginDetails)> _pluginsWithDetails;        

        public PluginManagerV1(ILoggerFactory loggerFactory, IPluginManagerConfiguration pluginManagerConfiguration)
        {
            _logger = loggerFactory.CreateLogger<PluginManagerV1>();
            _pluginManagerConfiguration = pluginManagerConfiguration;
            _pluginsWithDetails = new ConcurrentDictionary<string, (IPlugin, IPluginDetails)>();

            LoadPlugins();
        }


        #region Plugin Manager Details

        public IEnumerable<IPluginDetails> GetPluginDetails()
        {
            return _pluginsWithDetails.Select(x => x.Value.Item2);
        }

        public bool HasErroredPlugins
        {
            get
            {
                return _pluginsWithDetails.Where(x => x.Value.Item2.CurrentState == State.Errored).Any();
            }
        }

        #endregion Plugin Manager Details

        #region Start/Stop Methods

        public void Start()
        {
            Parallel.ForEach(_pluginsWithDetails, pluginWithDetails =>
            {
                try
                {
                    var instance = SetPluginToInitialState(pluginWithDetails.Value);

                }
                catch (Exception e)
                {
                    pluginWithDetails.Value.Item2.CurrentState = State.Errored;
                    _logger.LogCritical(e, $"Failed to Start plugin {pluginWithDetails.Key}");
                }
            });
        }

        public void Stop()
        {
            Parallel.ForEach(_pluginsWithDetails, pluginWithDetails =>
            {
                try
                {
                    pluginWithDetails.Value.Item1.Stop();
                    pluginWithDetails.Value.Item2.StoppedOn = DateTime.Now;
                    pluginWithDetails.Value.Item2.CurrentState = State.Stopped;
                }
                catch (Exception e)
                {
                    pluginWithDetails.Value.Item2.CurrentState = State.Errored;
                    _logger.LogCritical(e, $"Failed to Stop plugin {pluginWithDetails.Key}...attempting  Terminate");
                }
            });
        }

        public void Start(string pluginName)
        {
            try
            {
                if(_pluginsWithDetails.TryGetValue(pluginName, out (IPlugin, IPluginDetails) pluginWithDetails))
                {
                    if(pluginWithDetails.Item2.CurrentState != State.Started)
                    {
                        pluginWithDetails.Item1.Start();
                        pluginWithDetails.Value.Item2.StartedOn = DateTime.Now;
                        pluginWithDetails.Value.Item2.CurrentState = State.Started;
                    }
                    
                }
            }
            catch (Exception e)
            {
                plugin.Value.Item2.CurrentState = State.Errored;
                _logger.LogCritical(e, $"Failed to Stop plugin {plugin.Key}...attempting  Terminate");
            }
        }

        public void Stop(string pluginName)
        {
            throw new NotImplementedException();
        }

        #region Start/Stop Helpers

        private (IPlugin, IPluginDetails) SetPluginToInitialState((IPlugin, IPluginDetails) pluginWithDetails)
        {
            if(pluginWithDetails.Item2.CurrentState != State.Initialized)
            {// assume plugin has to be re-initialized
             //...check if we have to stop and free up current ref

                IPlugin newInstance = null;
                if(TryCreateInstance(pluginWithDetails.Item2.TypeInfo, out newInstance))
                {
                    pluginWithDetails.Item1 = newInstance;
                    pluginWithDetails.Item2.CurrentState = State.Initialized;
                }
                
            }

            return pluginWithDetails;
        }

        private bool TryStopInstance((IPlugin, IPluginDetails) pluginWithDetails)
        {
            bool hasStopped;
            try
            {
                pluginWithDetails.Item1.Stop();
                hasStopped = true;
            }
            catch(Exception e)
            {
                _logger.LogCritical(e, $"Failed to stop plugin {pluginWithDetails.Item2.Name}");
            }
        }
              
        #endregion Start/Stop Helpers

        #endregion Start/Stop Methods

        #region Load Methods

        private void LoadPlugins()
        {
            var pluginDirectories = GetPluginDirectories();
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

                tasks[i] = Task.Run(() => TryRegisterPlugin(pluginType));                 
            }

            return tasks;
        }

        private void TryRegisterPlugin(TypeInfo pluginType)
        {
            var details = new PluginDetails
            {
                Name = pluginType.Name,
                TypeInfo = pluginType,
                CurrentState = State.Initialized,
                InitializedOn = DateTime.Now
            };

            try
            {                
                if (TryCreateInstance(pluginType, out IPlugin plugin))
                {
                    if (_pluginsWithDetails.TryAdd(pluginType.FullName, (plugin, details)))
                    {
                        _logger.LogInformation($"Registered plugin instance {pluginType.FullName}");
                    }
                    else
                    {
                        _logger.LogInformation($"Unable to register plugin instance {pluginType.FullName}");
                    }
                }
                else
                {
                    _logger.LogInformation($"Failed to create plugin instance {pluginType.FullName}");
                }
            }
            catch(Exception e)
            {
                _logger.LogCritical(e, $"Unhandled error registering plugin {details.ToString()}");
            }
            
        }

        #endregion Load Methods

        #region Helper Methods

        private IEnumerable<string> GetPluginDirectories()
        {
            return Directory.GetDirectories(_pluginManagerConfiguration.PluginDirectoryRoot);
        }

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

        #endregion Helper Methods

    }
}
