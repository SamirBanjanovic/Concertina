using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Concertina.Core
{
    public interface IPluginManager
    {
        IEnumerable<(string, Stack<IPluginDetails>)> GetHistory();

        IEnumerable<string> GetErroredPlugins();

        bool HasErroredPlugins { get; }
        
        void Start();

        void Stop();

        void Start(string pluginName);

        void Stop(string pluginName);

        void ReInitialize(string pluginName);
    }
}
