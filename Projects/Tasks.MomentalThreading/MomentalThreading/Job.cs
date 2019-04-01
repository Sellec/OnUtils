using System;
using NCrontab;

namespace OnUtils.Tasks.MomentalThreading
{
    class Job
    {
        public string JobName { get; set; }

        public CrontabSchedule CronSchedule { get; set; }

        public Action ExecutionDelegate { get; set; }

        public DateTime ClosestOccurrence { get; set; }
    }
}
