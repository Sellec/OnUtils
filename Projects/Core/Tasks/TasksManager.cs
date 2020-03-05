using System;
using System.Linq;
using System.Linq.Expressions;

namespace OnUtils.Tasks
{
    /// <summary>
    /// Предоставляет доступ к общим функциям, связанным с фоновыми задачами.
    /// </summary>
    public static class TasksManager
    {
        #region ITasksService
        private static bool _testKnownService = true;
        private static ITasksService _defaultService = null;

        /// <summary>
        /// Устанавливает сервис <see cref="ITasksService"/> в качестве основного для работы с фоновыми задачами.
        /// </summary>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="service"/> равен null.</exception>
        public static void SetDefaultService(ITasksService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            _testKnownService = false;
            _defaultService = service;
            _defaultService.Initialize();
        }

        /// <summary>
        /// Возвращает текущий сервис <see cref="ITasksService"/> для работы с фоновыми задачами. Если возвращает null, значит, сервис не задан и при попытке вызова метода, связанного с задачами будет генерироваться исключение <see cref="InvalidOperationException"/>.
        /// Возвращает текущий провайдер данных.
        /// </summary>
        public static ITasksService GetDefaultService()
        {
            if (_defaultService == null && _testKnownService)
            {
                _testKnownService = false;
                var serviceType = Type.GetType("OnUtils.Tasks.Hangfire.TasksService, OnUtils.Tasks.Hangfire, Culture=neutral, PublicKeyToken=8e22adab863b765a", false);
                if (serviceType == null) serviceType = Type.GetType("OnUtils.Tasks.MomentalThreading.TasksService, OnUtils.Tasks.MomentalThreading, Culture=neutral, PublicKeyToken=8e22adab863b765a", false);
                if (serviceType != null) SetDefaultService((ITasksService)Activator.CreateInstance(serviceType));
            }

            return _defaultService;
        }
        #endregion

        #region Методы
        /// <summary>
        /// Устанавливает повторяющуюся задачу с именем <paramref name="name"/>, указанным расписанием <paramref name="cronExpression"/> на основе делегата <paramref name="taskDelegate"/>.
        /// Если задача с таким именем уже существует, то делегат и расписание будут обновлены.
        /// </summary>
        /// <exception cref="InvalidOperationException">Возникает, если не установлен сервис <see cref="ITasksService"/> (см. <see cref="SetDefaultService(ITasksService)"/>).</exception>
        public static void SetTask(string name, string cronExpression, Expression<Action> taskDelegate)
        {
            CheckDefaultService();
            GetDefaultService().SetTask(name, cronExpression, taskDelegate);
        }

        /// <summary>
        /// Устанавливает разовую задачу с именем <paramref name="name"/>, указанным временем запуска <paramref name="startTime"/> на основе делегата <paramref name="taskDelegate"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Возникает, если не установлен сервис <see cref="ITasksService"/> (см. <see cref="SetDefaultService(ITasksService)"/>).</exception>
        public static void SetTask(string name, DateTime startTime, Expression<Action> taskDelegate)
        {
            SetTask(name, new DateTimeOffset(startTime), taskDelegate);
        }

        /// <summary>
        /// Устанавливает разовую задачу с именем <paramref name="name"/>, указанным временем запуска <paramref name="startTime"/> на основе делегата <paramref name="taskDelegate"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Возникает, если не установлен сервис <see cref="ITasksService"/> (см. <see cref="SetDefaultService(ITasksService)"/>).</exception>
        public static void SetTask(string name, DateTimeOffset startTime, Expression<Action> taskDelegate)
        {
            CheckDefaultService();
            GetDefaultService().SetTask(name, startTime, taskDelegate);
        }

        /// <summary>
        /// Удаляет все существующие задачи.
        /// </summary>
        /// <exception cref="InvalidOperationException">Возникает, если не установлен сервис <see cref="ITasksService"/> (см. <see cref="SetDefaultService(ITasksService)"/>).</exception>
        public static void DeleteAllTasks()
        {
            CheckDefaultService();
            GetDefaultService().DeleteAllTasks();
        }

        /// <summary>
        /// Удаляет задачу с именем <paramref name="name"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Возникает, если не установлен сервис <see cref="ITasksService"/> (см. <see cref="SetDefaultService(ITasksService)"/>).</exception>
        public static void RemoveTask(string name)
        {
            CheckDefaultService();
            GetDefaultService().RemoveTask(name);
        }

        internal static void CheckDefaultService()
        {
            if (GetDefaultService() == null) throw new InvalidOperationException($"Не установлен сервис для работы с фоновыми задачами '{nameof(ITasksService)}'.");
        }
        #endregion
    }
}
