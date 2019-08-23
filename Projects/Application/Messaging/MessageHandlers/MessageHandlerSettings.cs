using System;

namespace OnUtils.Application.Messaging.MessageHandlers
{
    /// <summary>
    /// Хранит настройки обработчика сообщений.
    /// </summary>
    public class MessageHandlerSettings
    {
        /// <summary>
        /// Полное имя типа обработчика сообщений (см. <see cref="Type.FullName"/>).
        /// </summary>
        public string TypeFullName { get; set; }

        /// <summary>
        /// Настройки, сериализованные в строку. Способ сериализации зависит от обработчика сообщений.
        /// </summary>
        public string SettingsSerialized { get; set; }
    }
}
