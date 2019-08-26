using System;
using System.Collections.Generic;

namespace OnUtils.Application.Messaging.Components
{
    using Messages;

    /// <summary>
    /// Представляет компонент для получения и регистрации сообщений определенного типа.
    /// </summary>
    public interface IIncomingMessageReceiver<TAppCoreSelfReference, TMessage> : IMessageServiceComponent<TAppCoreSelfReference, TMessage>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        where TMessage : MessageBase, new()
    {
        /// <summary>
        /// Возвращает новые сообщения для регистрации в сервисе для дальнейшей обработки.
        /// </summary>
        /// <param name="service">Сервис обработки сообщений, в котором будут зарегистрированы новые сообщения.</param>
        /// <remarks>Дополнительные типы исключений, которые могут возникнуть во время получения сообщений, могут быть описаны в документации компонента.</remarks>
        [ApiIrreversible]
        List<MessageInfo<TMessage>> Receive(MessageServiceBase<TAppCoreSelfReference, TMessage> service);
    }
}
