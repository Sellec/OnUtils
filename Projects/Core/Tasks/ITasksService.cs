using System;
using System.Linq.Expressions;

namespace OnUtils.Tasks
{
    /// <summary>
    /// Представляет сервис для работы с фоновыми задачами.
    /// </summary>
    public interface ITasksService
    {
        /// <summary>
        /// Выполняет инициализацию сервиса. Не должен вызываться из пользовательского кода, т.к. автоматически вызывается из <see cref="TasksManager"/>.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Устанавливает повторяющуюся задачу с именем <paramref name="name"/>, указанным расписанием <paramref name="cronExpression"/> на основе делегата <paramref name="taskDelegate"/>.
        /// Если задача с таким именем уже существует, то делегат и расписание будут обновлены.
        /// </summary>
        void SetTask(string name, string cronExpression, Expression<Action> taskDelegate);

        /// <summary>
        /// Устанавливает разовую задачу с именем <paramref name="name"/>, указанным временем запуска <paramref name="startTime"/> на основе делегата <paramref name="taskDelegate"/>.
        /// </summary>
        void SetTask(string name, DateTimeOffset startTime, Expression<Action> taskDelegate);

        /// <summary>
        /// Удаляет все существующие задачи.
        /// </summary>
        void DeleteAllTasks();

        /// <summary>
        /// Удаляет задачу с именем <paramref name="name"/>.
        /// </summary>
        void RemoveTask(string name);
    }
}
