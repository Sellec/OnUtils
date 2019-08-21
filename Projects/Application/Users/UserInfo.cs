namespace OnUtils.Application.Users
{
    /// <summary>
    /// Хранит информацию о пользователе.
    /// </summary>
    public class UserInfo
    {
        internal UserInfo(DB.UserBase source)
        {
            IdUser = source.IdUser;
            IsSuperuser = source.IsSuperuser;
            UniqueKey = source.UniqueKey;
        }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public int IdUser { get; }

        /// <summary>
        /// Указывает, является ли суперпользователем с неограниченными правами.
        /// </summary>
        public bool IsSuperuser { get; }

        /// <summary>
        /// Уникальный ключ пользователя.
        /// </summary>
        public string UniqueKey { get; }
    }
}
