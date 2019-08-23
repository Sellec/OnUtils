using System;
using System.Collections.Generic;

namespace OnUtils.Application.Messaging.MessageHandlers
{
    /// <summary>
    /// Представляет обработчик для получения и регистрации сообщений определенного типа.
    /// </summary>
    public interface IMessageReceiver<TAppCoreSelfReference, TMessage> : IMessageHandler<TAppCoreSelfReference, TMessage>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        where TMessage : MessageBase, new()
    {
        #region Методы
        /// <summary>
        /// Возвращает новые сообщения для регистрации в сервисе для дальнейшей обработки.
        /// </summary>
        /// <param name="service">Сервис отправки сообщений, в котором будут зарегистрированы новые сообщения.</param>
        /// <remarks>Дополнительные типы исключений, которые могут возникнуть во время отправки сообщения, могут быть описаны в документации обработчика.</remarks>
        [ApiIrreversible]
        List<HandlerMessage<TMessage>> Receive(MessageServiceBase<TAppCoreSelfReference, TMessage> service);
        #endregion

    }
}
