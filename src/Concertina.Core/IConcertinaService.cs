using System;
using System.Collections.Generic;
using System.Text;

namespace Concertina.Core
{
    public interface IConcertinaService
    {
        IConcertinaConfiguration ServiceConfiguration { get; }

        void Start();

        void Stop();
    }
}
