using System;

namespace OnUtils.Application.Users
{
    using Architecture.AppCore;

    /// <summary>
    /// Менеджер, управляющий контекстами пользователей (см. <see cref="IUserContext"/>).
    /// Каждый поток приложения имеет ассоциированный контекст пользователя, от имени которого могут выполняться запросы и выполняться действия. 
    /// Более подробно см. <see cref="GetCurrentUserContext"/> / <see cref="SetCurrentUserContext(IUserContext)"/> / <see cref="ClearCurrentUserContext"/>.
    /// </summary>
    public class UserContextManager<TApplication> : CoreComponentBase<TApplication>, IComponentSingleton<TApplication>
        where TApplication : AppCore<TApplication>
    {
        private static IUserContext SystemUserContext { get; set; } = null;

        [ThreadStatic]
        private IUserContext _currentUserContext = null;

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStart()
        {
            var systemUserContext = new SimpleUserContext<TApplication>() { UserID = Guid.NewGuid(), IsGuest = false, IsSuperuser = true };
            systemUserContext.Start(AppCore);
            SystemUserContext = systemUserContext;
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
        }
        #endregion

        #region Методы
        /// <summary>
        /// Возвращает контекст системного пользователя.
        /// </summary>
        public virtual IUserContext GetSystemUserContext()
        {
            return SystemUserContext;
        }

        /// <summary>
        /// Возвращает контекст гостя.
        /// </summary>
        public virtual IUserContext CreateGuestUserContext()
        {
            var context = new SimpleUserContext<TApplication>() { UserID = Guid.NewGuid(), IsGuest = true, IsSuperuser = false };
            context.Start(AppCore);
            return context;
        }

        /// <summary>
        /// Возвращает контекст пользователя, ассоциированный с текущим потоком выполнения. 
        /// По-умолчанию возвращается контекст системного пользователя, если не задан иной контекст путем вызова <see cref="SetCurrentUserContext(IUserContext)"/>.
        /// </summary>
        public virtual IUserContext GetCurrentUserContext()
        {
            if (_currentUserContext == null) ClearCurrentUserContext();
            return _currentUserContext;
        }

        /// <summary>
        /// Устанавливает текущий контекст пользователя. Для замены текущего контекста достаточно заново вызвать этот метод, вызывать <see cref="ClearCurrentUserContext"/> для сброса контекста необязательно.
        /// </summary>
        /// <param name="context">Новый контекст пользователя. Не должен быть равен null.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="context"/> равен null.</exception>
        public virtual void SetCurrentUserContext(IUserContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            _currentUserContext = context;
        }

        /// <summary>
        /// Устанавливает контекст гостя в качестве текущего контекста, сбрасывая любой предыдущий установленный контекст.
        /// </summary>
        public virtual void ClearCurrentUserContext()
        {
            _currentUserContext = CreateGuestUserContext();
        }
        #endregion
    }
}
