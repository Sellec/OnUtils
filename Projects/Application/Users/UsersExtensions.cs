using System;

namespace OnUtils.Application
{
    using Application.Users;

    /// <summary>
    /// Методы расширений для раздела пользовательских менеджеров и контекстов.
    /// </summary>
    public static class UsersExtensions
    {
        /// <summary>
        /// Возвращает идентификатор пользователя, ассоциированного с контекстом. См. <see cref="DB.User.IdUser"/>.
        /// </summary> 
        public static int GetIdUser(this IUserContext context)
        {
            if (context is Users.UserContext coreContext) return coreContext.IdUser;
            throw new ArgumentException("Контекст пользователя не является контекстом, используемым в веб-ядре.", nameof(context));
        }

        /// <summary>
        /// Возвращает данные пользователя, ассоциированного с контекстом. См. <see cref="DB.User"/>.
        /// </summary>
        public static DB.User GetData(this IUserContext context)
        {
            if (context is UserContext coreContext) return coreContext.GetData();
            throw new ArgumentException("Контекст пользователя не является контекстом, используемым в веб-ядре.", nameof(context));
        }
    }
}
