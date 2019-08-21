using System;
using System.Collections.Generic;
using System.Text;

namespace OnUtils.Application.Modules
{
    /// <summary>
    /// Предоставляет варианты результата выполнения функции проверки прав доступа (см. <see cref="ModuleCore{TAppCoreSelfReference}.CheckPermission(Users.IUserContext, Guid)"/>).
    /// </summary>
    public enum CheckPermissionResult
    {
        /// <summary>
        /// Разрешено.
        /// </summary>
        Allowed,

        /// <summary>
        /// Запрещено.
        /// </summary>
        Denied,

        /// <summary>
        /// Разрешение не относится к модулю.
        /// </summary>
        PermissionNotFound,
    }
}
