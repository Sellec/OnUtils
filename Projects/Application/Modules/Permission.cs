using System;

namespace OnUtils.Application.Modules
{
    /// <summary>
    /// Представляет отдельное разрешение для модуля.
    /// </summary>
    public class Permission
    {
        /// <summary>
        /// Уникальный ключ разрешения. В рамках одного модуля не может существовать двух разрешений с одинаковым ключом.
        /// </summary>
        public Guid Key { get; internal set; }

        /// <summary>
        /// Название разрешения.
        /// </summary>
        public string Caption { get; internal set; }

        /// <summary>
        /// Описание разрешения.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Если true, то для суперпользователей не будет по-умолчанию возвращаться true для этого ресурса.
        /// </summary>
        public bool IgnoreSuperuser { get; internal set; }

        /// <summary>
        /// </summary>
        public override string ToString()
        {
            return Caption;
        }
    }
}
