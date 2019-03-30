using System;
using System.Collections.Generic;
using System.Text;

namespace OnUtils.Application.Modules
{
    /// <summary>
    /// Некоторые константы, связанные с модулями.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Обозначает разрешение на управление модулем.
        /// </summary>
        public const string PermissionManageString = "accessadmin";

        /// <summary>
        /// Обозначает разрешение для доступа авторизованного пользователя к модулю.
        /// </summary>
        public const string PermissionAccessUserString = "accessuser";

        /// <summary>
        /// Обозначает разрешение на управление модулем.
        /// </summary>
        public static readonly Guid PermissionManage = PermissionManageString.GenerateGuid();

        /// <summary>
        /// Обозначает разрешение для доступа авторизованного пользователя к модулю.
        /// </summary>
        public static readonly Guid PermissionAccessUser = PermissionAccessUserString.GenerateGuid();
    }
}
