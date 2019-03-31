using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Threading;

namespace OnUtils.Tasks.MomentalThreading
{
    using Data;
    using Tasks;

    /// <summary>
    /// Сервис для работы с фоновыми задачами на основе библиотеки Hangfire.
    /// </summary>
    public class TasksService : ITasksService, IDisposable
    {
        private bool _initialized = false;
        private Timer _jobsTimer = null;
        private object _jobsListSyncRoot = new object();
        private List<Task> _jobsList = new List<Task>();

        private System.Collections.Concurrent.ConcurrentQueue<Action> _actionsQueue = new System.Collections.Concurrent.ConcurrentQueue<Action>();

        void ITasksService.Initialize()
        {
            _jobsTimer = new Timer(new TimerCallback(state => CheckTasks()), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            CheckInitialization();
        }

        private void CheckTasks()
        {
            lock(_jobsListSyncRoot)
            {
                var jobsListSnapshot = _jobsList.ToList();
            }
        }

        private void CheckInitialization()
        {
            if (!_initialized)
            {
                Task.Factory.StartNew(async () => {
                    for (int i = 0; i < 5; i++)
                    {
                        try
                        {
                            var connectionStringResolver = DataAccessManager.GetConnectionStringResolver();
                            if (connectionStringResolver == null) throw new InvalidOperationException($"Невозможно получить строку подключения. Задайте поставщик строк подключения в {nameof(DataAccessManager)}.{nameof(DataAccessManager.SetConnectionStringResolver)}.");

                            var connString = connectionStringResolver.ResolveConnectionStringForDataContext(new Type[] { });
                            if (!string.IsNullOrEmpty(connString))
                            {
                                try
                                {
                                    using (var db = new Context())
                                    {
                                        db.DataContext.ExecuteQuery("IF OBJECT_ID (N'Hangfire.Job', N'U')  IS NOT NULL DELETE FROM Hangfire.Job");
                                    }
                                }
                                catch { }

                                GlobalConfiguration.Configuration.UseSqlServerStorage(connString);

                                _server = new BackgroundJobServer();

                                // Удаление всех запланированных задач. Учитывая, что сейчас задачи добавляются через AddOrUpdate, можно обойтись без регулярного удаления всех задач. Если будут возникать ошибочные задачи, их надо будет удалять через какой-то отдельный механизм.
                                // Пока что уберем удаление. Перенес его в DeleteAllTasks.
                                //try
                                //{
                                //    using (var connection = JobStorage.Current.GetConnection())
                                //    {
                                //        connection.GetRecurringJobs().ForEach(x => RecurringJob.RemoveIfExists(x.Id));
                                //    }
                                //}
                                //catch { }

                                Action action = null;
                                while (_actionsQueue.TryDequeue(out action)) action();

                                break;
                            }
                        }
                        catch (Exception ex) { Debug.WriteLine("Hangfire.Init error: {0}", ex.ToString()); }

                        await Task.Delay(5000);
                    }
                });

            }
        }

        void IDisposable.Dispose()
        {
            try
            {
                if (_server != null) _server.Dispose();
            }
            catch { }
        }

        void ITasksService.SetTask(string name, string cronExpression, Expression<Action> taskDelegate)
        {
            ExecuteAction(() => RecurringJob.AddOrUpdate(name, taskDelegate, cronExpression));
        }

        void ITasksService.SetTask(string name, DateTime startTime, Expression<Action> taskDelegate)
        {
            ExecuteAction(() =>
            {
                if (DateTime.Now < startTime)
                    BackgroundJob.Schedule(taskDelegate, DateTime.Now - startTime);
            });
        }

        /// <summary>
        /// Здесь удаляются только повторяющиеся задачи.
        /// TODO - придумать, как удалять одноразовые.
        /// </summary>
        void ITasksService.DeleteAllTasks()
        {
            ExecuteAction(() =>
            {
                try
                {
                    using (var connection = JobStorage.Current.GetConnection())
                    {
                        connection.GetRecurringJobs().ForEach(x => RecurringJob.RemoveIfExists(x.Id));
                    }
                }
                catch { }
            });
        }

        void ITasksService.RemoveTask(string name)
        {
            ExecuteAction(() => RecurringJob.RemoveIfExists(name));
        }

        private void ExecuteAction(Action actionDelegate)
        {
            if (_server == null) _actionsQueue.Enqueue(actionDelegate);
            else actionDelegate();
        }
    }
}
