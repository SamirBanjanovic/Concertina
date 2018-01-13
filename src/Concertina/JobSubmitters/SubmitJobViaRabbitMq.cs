using System;
using System.Collections.Generic;
using System.Text;
using Integration.Core;

namespace Integration.Egine.JobSubmitters
{
    public class SubmitJobViaRabbitMq
        : ISubmitJobRequest
    {
        public string Name { get => $"Job Submitter {nameof(SubmitJobViaRabbitMq)}"; }

        public void SendJobRequest()
        {
            throw new NotImplementedException();
        }
    }
}
