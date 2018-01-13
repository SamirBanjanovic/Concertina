using System;
using System.Collections.Generic;
using System.Text;

namespace Integration.Core.Entities
{
    public class ServiceHost
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public OperatingSystem OperatingSystem { get; set; }
    }
}
