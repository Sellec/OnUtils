using System;

namespace OnUtils.Application.Messaging.MessageHandlers
{
    /// <summary>
    /// Представляет обработчик для отправки сообщений определенного типа.
    /// </summary>
    public interface IMessageSender<TAppCoreSelfReference, TMessage> : IMessageHandler<TAppCoreSelfReference, TMessage>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        where TMessage : MessageBase, new()
    {
        #region Методы
        /// <summary>
        /// Отправляет указанное сообщение.
        /// </summary>
        /// <param name="message">Информация о сообщении, которое необходимо отправить</param>
        /// <param name="service">Сервис отправки сообщений, которому принадлежит отправляемое сообщение <paramref name="message"/>.</param>
        /// <remarks>Дополнительные типы исключений, которые могут возникнуть во время отправки сообщения, могут быть описаны в документации обработчика.</remarks>
        [ApiIrreversible]
        void Send(HandlerMessage<TMessage> message, MessageServiceBase<TAppCoreSelfReference, TMessage> service);
        #endregion

    }
}
