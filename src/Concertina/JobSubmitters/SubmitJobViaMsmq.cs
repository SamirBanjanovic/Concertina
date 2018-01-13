using System;
using System.Collections.Generic;
using System.Text;
using Integration.Core;

namespace Integration.Egine.JobSubmitters
{
    public class SubmitJobViaMsmq
        : ISubmitJobRequest
    {
        public string Name { get => $"Job Submitter {nameof(SubmitJobViaMsmq)}"; }

        public void SendJobRequest()
        {
            throw new NotImplementedException();
        }
    }
}
