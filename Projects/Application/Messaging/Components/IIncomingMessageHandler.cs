using System;

namespace OnUtils.Application.Messaging.Components
{
    using Messages;

    /// <summary>
    /// Представляет компонент для обработки зарегистрированных входящих сообщений определенного типа.
    /// </summary>
    public interface IIncomingMessageHandler<TAppCoreSelfReference, TMessage> : IMessageServiceComponent<TAppCoreSelfReference, TMessage>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        where TMessage : MessageBase, new()
    {
        /// <summary>
        /// Обрабатывает указанное сообщение.
        /// </summary>
        /// <param name="message">Информация о сообщении, которое необходимо обработать</param>
        /// <param name="service">Сервис обработки сообщений, которому принадлежит сообщение <paramref name="message"/>.</param>
        /// <returns>Если возвращает true, то сообщение считается обработанным (см. <see cref="MessageStateType.Completed"/>).</returns>
        /// <remarks>Дополнительные типы исключений, которые могут возникнуть во время обработки сообщения, могут быть описаны в документации компонента.</remarks>
        [ApiIrreversible]
        bool Prepare(MessageInfo<TMessage> message, MessageServiceBase<TAppCoreSelfReference, TMessage> service);
    }
}
