using System;
using System.Collections.Generic;
using System.Text;
using Integration.Core;

namespace Integration.Engine.JobSubmitters
{
    public class SubmitJobViaApi
        : ISubmitJobRequest
    {
        public string Name { get => $"Job Submitter {nameof(SubmitJobViaApi)}"; }

        public void SendJobRequest()
        {
            throw new NotImplementedException();
        }
    }
}
