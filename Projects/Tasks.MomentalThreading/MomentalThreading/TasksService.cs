using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Threading;
using NCrontab;

namespace OnUtils.Tasks.MomentalThreading
{
    using Data;
    using Tasks;

    /// <summary>
    /// Сервис для работы с фоновыми задачами на основе библиотеки Hangfire.
    /// </summary>
    public class TasksService : ITasksService, IDisposable
    {
        private Timer _jobsTimer = null;
        private object _jobsSyncRoot = new object();
        private List<Job> _jobsList = new List<Job>();

        void ITasksService.Initialize()
        {
            _jobsTimer = new Timer(new TimerCallback(state => CheckTasks()), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        private void CheckTasks()
        {
            lock(_jobsSyncRoot)
            {
                var jobsListSnapshot = _jobsList.Where(job => job.ClosestOccurrence <= DateTime.Now).ToList();
                _jobsList.RemoveAll(job => job.CronSchedule == null && job.ClosestOccurrence <= DateTime.Now);
                _jobsList.Where(job => job.CronSchedule != null).ForEach(job => job.ClosestOccurrence = job.CronSchedule.GetNextOccurrence(DateTime.Now));

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
                    //throw new InvalidOperationException("Задача с указанным именем уже существует.");
                    _jobsList.RemoveAll(x => x.JobName == name);

                var schedule = CrontabSchedule.Parse(cronExpression);
                _jobsList.Add(new Job()
                {
                    CronSchedule = schedule,
                    JobName = name,
                    ExecutionDelegate = taskDelegate.Compile(), // todo добавить проверки на тип тела лямбды.
                    ClosestOccurrence = schedule.GetNextOccurrence(DateTime.Now) // todo разобраться с UtcTime.
                });
            }
        }

        void ITasksService.SetTask(string name, DateTime startTime, Expression<Action> taskDelegate)
        {
            lock (_jobsSyncRoot)
            {
                if (_jobsList.Any(x => x.JobName == name))
                    //throw new InvalidOperationException("Задача с указанным именем уже существует.");
                    _jobsList.RemoveAll(x => x.JobName == name);

                if (startTime < DateTime.Now) return;

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
