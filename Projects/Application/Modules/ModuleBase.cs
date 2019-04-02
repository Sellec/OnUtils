using System;
using System.Collections.Generic;

namespace OnUtils.Application.Modules
{
    using Architecture.AppCore;

    /// <summary>
    /// Базовый класс для всех модулей. Обязателен при реализации любых модулей, т.к. при задании привязок в DI проверяется наследование именно от этого класса.
    /// </summary>
    public abstract class ModuleBase<TApplication> : CoreComponentBase<TApplication>, IComponentSingleton<TApplication>
        where TApplication : AppCore<TApplication>
    {
        private Dictionary<Guid, Permission> _permissions = new Dictionary<Guid, Permission>();

        /// <summary>
        /// Создает новый экземпляр модуля.
        /// </summary>
        public ModuleBase()
        {
        }

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStart()
        {
            OnModuleStart();
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
            OnModuleStop();
        }
        #endregion

        #region Блок функций прав доступа
        /// <summary>
        /// Возвращает список разрешений модуля.
        /// </summary>
        public IEnumerable<Permission> GetPermissions()
        {
            return _permissions.Values;
        }

        /// <summary>
        /// Регистрация разрешения для системы доступа. Если разрешение с таким ключом <paramref name="key"/> уже существует, оно будет перезаписано новым.
        /// </summary>
        /// <param name="key">См. <see cref="Permission.Key"/>.</param>
        /// <param name="caption">См. <see cref="Permission.Caption"/>.</param>
        /// <param name="description">См. <see cref="Permission.Description"/>.</param>
        /// <param name="ignoreSuperuser">См. <see cref="Permission.IgnoreSuperuser"/>.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="caption"/> является пустой строкой или null.</exception>
        public void RegisterPermission(Guid key, string caption, string description = null, bool ignoreSuperuser = false)
        {
            if (string.IsNullOrEmpty(caption)) throw new ArgumentNullException(nameof(caption));
            _permissions[key] = new Permission() { Caption = caption, Description = description, IgnoreSuperuser = ignoreSuperuser };
        }

        /// <summary>
        /// Проверяет, доступно ли указанное разрешение <paramref name="key"/> пользователю, ассоциированному с текущим контекстом (см. <see cref="Users.UserContextManager{TApplication}.GetCurrentUserContext"/>).
        /// </summary>
        /// <param name="key">Уникальный ключ разрешения. См. <see cref="Permission.Key"/>.</param>
        /// <returns>Возвращает результат проверки.</returns>
        public CheckPermissionResult CheckPermission(Guid key)
        {
            return CheckPermission(AppCore.Get<Users.UserContextManager<TApplication>>().GetCurrentUserContext(), key);
        }

        /// <summary>
        /// Проверяет, доступно ли указанное разрешение <paramref name="key"/> пользователю, ассоциированному с контекстом <paramref name="context"/>.
        /// </summary>
        /// <param name="context">Контекст пользователя.</param>
        /// <param name="key">Уникальный ключ разрешения. См. <see cref="Permission.Key"/>.</param>
        /// <returns>Возвращает результат проверки.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="context"/> равен null.</exception>
        public CheckPermissionResult CheckPermission(Users.IUserContext context, Guid key)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            Permission permData = null;
            if (!_permissions.TryGetValue(key, out permData) && key != ModulesConstants.PermissionAccessUser) return CheckPermissionResult.PermissionNotFound;
            if (key == ModulesConstants.PermissionAccessUser && !context.IsGuest) return CheckPermissionResult.Allowed;
            if (!permData.IgnoreSuperuser && context.IsSuperuser) return CheckPermissionResult.Allowed;

            var userperms = context.Permissions;
            if (userperms == null || !userperms.ContainsKey(ModuleID))
            {
                return CheckPermissionResult.Denied;
            }
            else
            {
                var usermoduleperms = userperms[ModuleID];
                return usermoduleperms != null && usermoduleperms.Contains(key) ? CheckPermissionResult.Allowed : CheckPermissionResult.Denied;
            }
        }
        #endregion

        #region Для переопределения в наследниках
        /// <summary>
        /// Вызывается при запуске модуля.
        /// </summary>
        internal protected virtual void OnModuleStart()
        {

        }

        /// <summary>
        /// Вызывается при остановке модуля.
        /// </summary>
        internal protected virtual void OnModuleStop()
        {

        }
        #endregion

        #region Свойства
        /// <summary>
        /// Возвращает идентификатор модуля.
        /// </summary>
        public virtual Guid ModuleID { get; }
        #endregion
    }
}
