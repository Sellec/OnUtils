using NCrontab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace OnUtils.Tasks.MomentalThreading
{
    using Tasks;

    /// <summary>
    /// Сервис для работы с фоновыми задачами на основе библиотеки Hangfire.
    /// </summary>
    public class TasksService : ITasksService, IDisposable
    {
        private Timer _jobsTimer = null;
        private object _jobsSyncRoot = new object();
        private List<Job> _jobsList = new List<Job>();
        private Guid _unique = Guid.NewGuid();

        void ITasksService.Initialize()
        {
            _jobsTimer = new Timer(new TimerCallback(state => CheckTasks()), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        private void CheckTasks()
        {
            lock (_jobsSyncRoot)
            {
                var jobsListSnapshot = _jobsList.Where(job => job.ClosestOccurrence <= DateTimeOffset.Now).ToList();
                _jobsList.RemoveAll(job => job.CronSchedule == null && job.ClosestOccurrence <= DateTimeOffset.Now);
                _jobsList.Where(job => job.CronSchedule != null).ForEach(job => job.ClosestOccurrence = job.CronSchedule.GetNextOccurrence(DateTime.UtcNow));

                jobsListSnapshot.ForEach(job => Task.Factory.StartNew(() => job.ExecutionDelegate()));
            }
        }

        void IDisposable.Dispose()
        {
            lock (_jobsSyncRoot)
            {
                try { _jobsList.Clear(); }
                catch { }

                try { if (_jobsTimer != null) _jobsTimer.Change(Timeout.Infinite, Timeout.Infinite); }
                catch { }
            }
        }

        void ITasksService.SetTask(string name, string cronExpression, Expression<Action> taskDelegate)
        {
            lock (_jobsSyncRoot)
            {
                if (_jobsList.Any(x => x.JobName == name))
                    _jobsList.RemoveAll(x => x.JobName == name);

                var schedule = CrontabSchedule.Parse(cronExpression);
                _jobsList.Add(new Job()
                {
                    CronSchedule = schedule,
                    JobName = name,
                    ExecutionDelegate = taskDelegate.Compile(), // todo добавить проверки на тип тела лямбды.
                    ClosestOccurrence = schedule.GetNextOccurrence(DateTime.UtcNow)
                });
            }
        }

        void ITasksService.SetTask(string name, DateTimeOffset startTime, Expression<Action> taskDelegate)
        {
            lock (_jobsSyncRoot)
            {
                if (_jobsList.Any(x => x.JobName == name))
                    _jobsList.RemoveAll(x => x.JobName == name);

                if (startTime < DateTimeOffset.Now) return;

                _jobsList.Add(new Job()
                {
                    CronSchedule = null,
                    JobName = name,
                    ExecutionDelegate = taskDelegate.Compile(),
                    ClosestOccurrence = startTime
                });
            }
        }

        void ITasksService.DeleteAllTasks()
        {
            lock (_jobsSyncRoot)
            {
                _jobsList.Clear();
            }
        }

        void ITasksService.RemoveTask(string name)
        {
            lock (_jobsSyncRoot)
            {
                _jobsList.RemoveAll(x => x.JobName == name);
            }
        }

    }
}
