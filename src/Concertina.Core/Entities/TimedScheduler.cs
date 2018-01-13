using System;
using System.Collections.Generic;
using System.Text;

namespace Integration.Core.Entities
{
    public class TimedScheduler
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string CronSchedule { get; set; }
    }
}
