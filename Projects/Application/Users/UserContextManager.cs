using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Transactions;

namespace OnUtils.Application.Users
{
    using Architecture.AppCore;
    using Data;
    using ExecutionPermissionsResult = ExecutionResult<UserPermissions>;

    /// <summary>
    /// Менеджер, управляющий контекстами пользователей (см. <see cref="IUserContext"/>).
    /// Каждый поток приложения имеет ассоциированный контекст пользователя, от имени которого могут выполняться запросы и выполняться действия. 
    /// Более подробно см. <see cref="UserContextManager{TApplication}.GetCurrentUserContext"/> / <see cref="UserContextManager{TApplication}.SetCurrentUserContext(IUserContext)"/> / <see cref="UserContextManager{TApplication}.ClearCurrentUserContext"/>.
    /// </summary>
    public class UserContextManager : CoreComponentBase<ApplicationCore>, IComponentSingleton<ApplicationCore>, IUnitOfWorkAccessor<DB.CoreContext>
    {
        public const string RoleUserName = "RoleUser";
        public const string RoleGuestName = "RoleGuest";

        private static IUserContext _systemUserContext;
        private ThreadLocal<IUserContext> _currentUserContext = new ThreadLocal<IUserContext>();

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStart()
        {
            var systemUserContext = new UserContext(new DB.UserBase() { IdUser = int.MaxValue - 1, IsSuperuser = true }, true);
            systemUserContext.Start(AppCore);
            _systemUserContext = systemUserContext;
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
            return _systemUserContext;
        }

        /// <summary>
        /// Возвращает контекст пользователя, ассоциированный с текущим потоком выполнения. 
        /// По-умолчанию возвращается контекст системного пользователя, если не задан иной контекст путем вызова <see cref="SetCurrentUserContext(IUserContext)"/>.
        /// </summary>
        public virtual IUserContext GetCurrentUserContext()
        {
            if (!_currentUserContext.IsValueCreated) ClearCurrentUserContext();
            return _currentUserContext.Value;
        }

        /// <summary>
        /// Устанавливает текущий контекст пользователя. Для замены текущего контекста достаточно заново вызвать этот метод, вызывать <see cref="ClearCurrentUserContext"/> для сброса контекста необязательно.
        /// </summary>
        /// <param name="context">Новый контекст пользователя. Не должен быть равен null.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="context"/> равен null.</exception>
        public virtual void SetCurrentUserContext(IUserContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            _currentUserContext.Value = context;
        }

        /// <summary>
        /// Устанавливает контекст гостя в качестве текущего контекста, сбрасывая любой предыдущий установленный контекст.
        /// </summary>
        public virtual void ClearCurrentUserContext()
        {
            _currentUserContext.Value = CreateGuestUserContext();
        }

        #region Создать контекст
        /// <summary>
        /// Возвращает контекст гостя.
        /// </summary>
        public virtual IUserContext CreateGuestUserContext()
        {
            return new UserContext(new DB.UserBase() { IdUser = 0, IsSuperuser = false }, false);
        }

        /// <summary>
        /// Возвращает контекст пользователя с идентификатором <paramref name="idUser"/>.
        /// </summary>
        /// <param name="idUser">Идентификатор пользователя.</param>
        /// <param name="userContext">Содержит контекст в случае успеха.</param>
        /// <param name="resultReason">Содержит текстовое пояснение к ответу функции.</param>
        /// <returns>Возвращает результат создания контекста.</returns>
        [ApiIrreversible]
        public UserContextCreateResult CreateUserContext(int idUser, out IUserContext userContext)
        {
            userContext = null;

            using (var db = new DB.CoreContext())
            using (var scope = db.CreateScope(TransactionScopeOption.RequiresNew))
            {
                try
                {
                    var res = db.Users.Where(x => x.IdUser == idUser).FirstOrDefault();
                    if (res == null) return UserContextCreateResult.NotFound;

                    var context = new UserContext(res, true);
                    context.Start(AppCore);

                    var permissionsResult = GetPermissions(context.IdUser);
                    if (!permissionsResult.IsSuccess)
                    {
                        return UserContextCreateResult.ErrorReadingPermissions;
                    }
                    context.ApplyPermissions(permissionsResult.Result);
                    userContext = context;
                    return UserContextCreateResult.Success;
                }
                catch (Exception ex)
                {
                    this.RegisterEvent(Journaling.EventType.CriticalError, "Неизвестная ошибка во время создания контекста пользователя.", $"IdUser={idUser}'.", null, ex);
                    userContext = null;
                    return UserContextCreateResult.ErrorUnknown;
                }
                finally
                {
                    scope.Commit();
                }
            }
        }
        #endregion

        public void DestroyUserContext(IUserContext context)
        {

        }
        #endregion

        #region Разрешения
        /// <summary>
        /// Возвращает список разрешений для пользователя <paramref name="idUser"/>.
        /// </summary>
        /// <returns>Возвращает объект <see cref="ExecutionPermissionsResult"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        [ApiIrreversible]
        public ExecutionPermissionsResult GetPermissions(int idUser)
        {
            try
            {
                using (var db = this.CreateUnitOfWork())
                using (var scope = db.CreateScope(TransactionScopeOption.Suppress))
                {
                    var idRoleUser = AppCore.Config.RoleUser;
                    var idRoleGuest = AppCore.Config.RoleGuest;

                    var perms2 = (from p in db.RolePermission
                                  join ru in db.RoleUser on p.IdRole equals ru.IdRole into gj
                                  from subru in gj.DefaultIfEmpty()
                                  where (subru.IdUser == idUser) || (idUser > 0 && p.IdRole == idRoleUser) || (idUser == 0 && p.IdRole == idRoleGuest)
                                  select new { p.IdModule, p.Permission });

                    var perms = new Dictionary<Guid, List<Guid>>();
                    foreach (var res in perms2)
                    {
                        if (!string.IsNullOrEmpty(res.Permission))
                        {
                            var guidModule = GuidIdentifierGenerator.GenerateGuid(GuidType.Module, res.IdModule);
                            var guidPermission = res.Permission.GenerateGuid();

                            if (!perms.ContainsKey(guidModule)) perms.Add(guidModule, new List<Guid>());
                            if (!perms[guidModule].Contains(guidPermission)) perms[guidModule].Add(guidPermission);
                        }
                    }

                    return new ExecutionPermissionsResult(true, null, new UserPermissions(perms));
                }
            }
            catch (Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.Error, "Ошибка при получении разрешений для пользователя.", $"IdUser={idUser}.", null, ex);
                return new ExecutionPermissionsResult(false, "Ошибка при получении разрешений для пользователя.");
            }
        }

        /// <summary>
        /// Пытается получить текущие разрешения для пользователя, ассоциированного с контекстом <paramref name="context"/>, и задать их контексту.
        /// </summary>
        /// <returns>Возвращает true, если удалось получить разрешения и установить их для переданного контекста.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="context"/> равен null.</exception>
        [ApiIrreversible]
        public virtual ExecutionResult TryRestorePermissions(IUserContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context is UserContext userContext)
            {
                var permissionsResult = GetPermissions(context.IdUser);
                if (permissionsResult.IsSuccess)
                {
                    userContext.ApplyPermissions(permissionsResult.Result);
                    return new ExecutionResult(true);
                }
                else return new ExecutionResult(false, permissionsResult.Message);
            }
            else return new ExecutionResult(false, "Неподдерживаемый тип контекста.");
        }
        #endregion
    }
}
