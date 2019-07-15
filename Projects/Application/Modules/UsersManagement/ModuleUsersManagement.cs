﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace OnUtils.Application.Modules.UsersManagement
{
    using DB;
    using Journaling;

    /// <summary>
    /// Модуль управления данными пользователей и системой доступа.
    /// </summary>
    [ModuleCore("Управление пользователями и доступом")]
    public sealed class ModuleUsersManagement<TAppCoreSelfReference> : ModuleCore<TAppCoreSelfReference, ModuleUsersManagement<TAppCoreSelfReference>>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        /// <summary>
        /// Возвращает список ролей, в которых есть разрешение <paramref name="permissionKey"/> в модуле <typeparamref name="TModule"/>.
        /// </summary>
        /// <param name="permissionKey">См. <see cref="Permission.Key"/>.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="permissionKey"/> является пустой строкой или null.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если в модуле <typeparamref name="TModule"/> не зарегистрировано разрешение <paramref name="permissionKey"/>.</exception>
        public List<Role> GetRolesByPermission<TModule>(string permissionKey)
            where TModule : ModuleCore<TAppCoreSelfReference, TModule>
        {
            if (string.IsNullOrEmpty(permissionKey)) throw new ArgumentNullException(nameof(permissionKey));

            var module = AppCore.Get<TModule>();
            if (module == null) throw new InvalidOperationException("Указанный модуль не найден.");

            if (!module.GetPermissions().Any(x => x.Key == permissionKey.GenerateGuid())) throw new InvalidOperationException($"Модуль '{module.Caption}' не содержит разрешение '{permissionKey}'.");

            try
            {
                using (var db = new CoreContext())
                {
                    var query = from permission in db.RolePermission
                                join role in db.Role on permission.IdRole equals role.IdRole
                                where permission.Permission == permissionKey && permission.IdModule == module.IdModule
                                select role;

                    var data = query.ToList();
                    return data;
                }
            }
            catch (Exception ex)
            {
                this.RegisterEvent(EventType.Error, "Ошибка получения списка ролей", $"Список ролей для разрешения '{permissionKey}'.", ex);
                throw;
            }
        }

        /// <summary>
        /// Возвращает список пользователей, облаюающих ролями с разрешением <paramref name="permissionKey"/> в модуле <typeparamref name="TModule"/>.
        /// </summary>
        /// <param name="permissionKey">См. <see cref="Permission.Key"/>.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="permissionKey"/> является пустой строкой или null.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если в модуле <typeparamref name="TModule"/> не зарегистрировано разрешение <paramref name="permissionKey"/>.</exception>
        public List<UserBase> GetUsersByRolePermission<TModule>(string permissionKey)
            where TModule : ModuleCore<TAppCoreSelfReference, TModule>
        {
            if (string.IsNullOrEmpty(permissionKey)) throw new ArgumentNullException(nameof(permissionKey));

            var module = AppCore.Get<TModule>();
            if (module == null) throw new InvalidOperationException("Указанный модуль не найден.");

            if (!module.GetPermissions().Any(x => x.Key == permissionKey.GenerateGuid())) throw new InvalidOperationException($"Модуль '{module.Caption}' не содержит разрешение '{permissionKey}'.");

            try
            {
                using (var db = new CoreContext())
                {
                    var query = from permission in db.RolePermission
                                join role in db.Role on permission.IdRole equals role.IdRole
                                join userRole in db.RoleUser on role.IdRole equals userRole.IdRole
                                join user in db.Users on userRole.IdUser equals user.IdUser
                                where permission.Permission == permissionKey && permission.IdModule == module.IdModule
                                select user;

                    var data = query.ToList();
                    return data;
                }
            }
            catch (Exception ex)
            {
                this.RegisterEvent(EventType.Error, "Ошибка получения списка пользователей", $"Список пользователей для разрешения '{permissionKey}'.", ex);
                throw;
            }
        }


    }
}
