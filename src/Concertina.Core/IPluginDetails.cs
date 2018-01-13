using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Concertina.Core
{
    public interface IPluginDetails
    {
        string Name { get; }

        TypeInfo TypeInfo { get; }                        

        PluginState State { get; }

        DateTime TimeStamp { get; }              
    }
}
