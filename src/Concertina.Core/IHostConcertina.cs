﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Concertina.Core
{
    public interface IHostConcertina
    {
        IConcertinaConfiguration ServiceConfiguration { get; }

    }
}
