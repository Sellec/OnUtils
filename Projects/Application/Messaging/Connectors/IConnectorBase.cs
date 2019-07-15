using System;

namespace OnUtils.Application.Messaging.Connectors
{
    using Architecture.AppCore;
    using Architecture.ObjectPool;

    /// <summary>
    /// Представляет коннектор к сервису отправки или получения сообщений.
    /// </summary>
    public interface IConnectorBase<TAppCoreSelfReference, TMessage> : IPoolObjectOrdered, IComponentTransient<TAppCoreSelfReference> 
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        where TMessage : MessageBase, new()
    {
        #region Методы
        /// <summary>
        /// Инициализация коннектора. При инициализации в качестве аргумента передается строка <paramref name="connectorSettings"/> с сериализованными настройками коннектора.
        /// </summary>
        /// <remarks>Дополнительные типы исключений, которые могут возникнуть во время инициализации, могут быть описаны в документации коннектора.</remarks>
        bool Init(string connectorSettings);

        /// <summary>
        /// Отправляет указанное сообщение.
        /// </summary>
        /// <param name="message">Информация о сообщении, которое необходимо отправить</param>
        /// <param name="service">Сервис отправки сообщений, которому принадлежит отправляемое сообщение <paramref name="message"/>.</param>
        /// <remarks>Дополнительные типы исключений, которые могут возникнуть во время отправки сообщения, могут быть описаны в документации коннектора.</remarks>
        [ApiIrreversible]
        void Send(ConnectorMessage<TMessage> message, IMessagingService<TAppCoreSelfReference> service);
        #endregion

        #region Свойства
        /// <summary>
        /// Возвращает название коннектора.
        /// </summary>
        string ConnectorName { get; }
        #endregion
    }
}
