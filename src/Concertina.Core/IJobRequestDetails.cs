using System;
using System.Collections.Generic;
using System.Text;

namespace Integration.Core
{
    public interface IJobRequestDetails
    {
        int RequestSourceId { get; set; }

        string RequestSource { get; set; }
    }
}
