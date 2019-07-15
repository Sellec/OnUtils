using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnUtils.Application.Configuration
{
    /// <summary>
    /// Класс конфигурации. При создании экземпляра объекта через метод Create ядра <see cref="ApplicationCore"/> автоматически заполняется значениями настроек ядра.
    /// </summary>
#pragma warning disable CS1591 // todo внести комментарии.
    public class CoreConfiguration<TAppCoreSelfReference> : ModuleConfiguration<TAppCoreSelfReference, Modules.CoreModule.CoreModule<TAppCoreSelfReference>>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        /// <summary>
        /// Основной системный язык.
        /// </summary>
        [Display(Name = "Основной системный язык")]
        public int IdSystemLanguage
        {
            get => Get("IdSystemLanguage", -1);
            set => Set("IdSystemLanguage", value);
        }

        /// <summary>
        /// Настройки коннекторов приёма/отправки сообщений.
        /// </summary>
        /// <seealso cref="Messaging.Connectors.IConnectorBase{TMessage}"/>
        /// <seealso cref="Messaging.MessagingManager"/>
        public List<Messaging.Connectors.ConnectorSettings> ConnectorsSettings
        {
            get => JsonConvert.DeserializeObject<List<Messaging.Connectors.ConnectorSettings>>(Get("ConnectorsSettings", "")) ?? new List<Messaging.Connectors.ConnectorSettings>();
            set => Set("ConnectorsSettings", value == null ? "" : JsonConvert.SerializeObject(value));
        }

        /// <summary>
        /// Роль, присваиваемая контексту пользователя. Это позволяет задавать права по-умолчанию для всех пользователей.
        /// </summary>
        public int RoleUser
        {
            get => Get(Users.UserContextManager<TAppCoreSelfReference>.RoleUserName, 0);
            set => Set(Users.UserContextManager<TAppCoreSelfReference>.RoleUserName, value);
        }

        /// <summary>
        /// Роль, присваиваемая контексту гостя. Это позволяет задавать права по-умолчанию для всех гостей.
        /// </summary>
        public int RoleGuest
        {
            get => Get(Users.UserContextManager<TAppCoreSelfReference>.RoleGuestName, 0);
            set => Set(Users.UserContextManager<TAppCoreSelfReference>.RoleGuestName, value);
        }
    }
}
