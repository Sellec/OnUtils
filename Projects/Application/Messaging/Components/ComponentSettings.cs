using System;

namespace OnUtils.Application.Messaging.Components
{
    /// <summary>
    /// Хранит настройки компонента сервиса отправки сообщений.
    /// </summary>
    public class ComponentSettings
    {
        /// <summary>
        /// Полное имя типа компонента (см. <see cref="Type.FullName"/>).
        /// </summary>
        public string TypeFullName { get; set; }

        /// <summary>
        /// Настройки, сериализованные в строку. Способ сериализации зависит от компонента.
        /// </summary>
        public string SettingsSerialized { get; set; }
    }
}
