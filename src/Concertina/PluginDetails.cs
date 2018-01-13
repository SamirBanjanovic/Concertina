using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Concertina.Core;

namespace Concertina
{
    public sealed class PluginDetails
        : IPluginDetails
    {
        public PluginDetails(string name, TypeInfo typeInfo, PluginState state)
        {
            Name = name;
            TypeInfo = typeInfo;
            State = state;
            TimeStamp = DateTime.Now;
        }

        public string Name { get; }        
        public TypeInfo TypeInfo { get; }
        public PluginState State { get; }
        public DateTime TimeStamp { get; }
        public override string ToString()
        {
            return Name;
        }
    }
}
