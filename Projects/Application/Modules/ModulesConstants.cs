using System;

namespace OnUtils.Application.Modules
{
    /// <summary>
    /// Некоторые константы, связанные с модулями.
    /// </summary>
    public static class ModulesConstants
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

        /// <summary>
        /// Обозначает ключ разрешения для сохранения настроек модуля.
        /// </summary>
        public static readonly Guid PermissionSaveConfiguration = "perm_configSave".GenerateGuid();

        public const int CategoryType = 1;

        public const int ItemType = 2;
    }
}
