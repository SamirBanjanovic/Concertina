using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Concertina.Core
{
    public interface IPlugin
    {
        void Start();
        
        void Stop();

        void Start(int resourceId);
        
        void Stop(int resourceId);

        void Terminate();
    }
}
