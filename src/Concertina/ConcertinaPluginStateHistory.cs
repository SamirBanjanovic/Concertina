using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concertina.Core;
using System.Reflection;

namespace Concertina
{
    internal static class ConcertinaPluginStateHistory
    {
        private static readonly ConcurrentDictionary<string, Stack<IPluginDetails>> _history = new ConcurrentDictionary<string, Stack<IPluginDetails>>();

        public static IEnumerable<(string, Stack<IPluginDetails>)> GetHistory() => _history.Select(x => (x.Key, new Stack<IPluginDetails>(x.Value))).ToList();

        public static IPluginDetails SetState(this IPlugin plugin, PluginState state)
        {
            var details = new PluginDetails(plugin.GetType().FullName, plugin.GetType().GetTypeInfo(), state);

            if(_history.TryGetValue(details.Name, out Stack<IPluginDetails> pastDetails))
            {
                pastDetails.Push(details);                
            }
            else
            {
                var past = new Stack<IPluginDetails>();
                past.Push(details);

                // for now assume it'll be added ... 
                // later we'll had a retry queue and more robustness
                // much later
                _history.TryAdd(details.Name, past);
            }

            return details;
        }
    }
}
