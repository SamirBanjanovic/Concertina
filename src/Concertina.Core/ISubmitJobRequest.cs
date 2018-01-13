using System;
using System.Collections.Generic;
using System.Text;

namespace Integration.Core
{
    public interface ISubmitJobRequest
    {
        string Name { get; }

        void SendJobRequest(IJobRequestDetails jobRequestDetails);
    }
}
