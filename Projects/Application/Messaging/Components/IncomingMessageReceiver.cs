using System;
using System.Collections.Generic;

namespace OnUtils.Application.Messaging.Components
{
    using Messages;

    /// <summary>
    /// Базовый класс компонента для получения и регистрации сообщений определенного типа.
    /// </summary>
    public abstract class IncomingMessageReceiver<TAppCoreSelfReference, TMessage> : MessageServiceComponent<TAppCoreSelfReference, TMessage>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        where TMessage : MessageBase, new()
    {
        /// <summary>
        /// Создает новый экземпляр компонента.
        /// </summary>
        protected IncomingMessageReceiver() : this(null, null)
        {
        }

        /// <summary>
        /// Создает новый экземпляр компонента.
        /// </summary>
        /// <param name="name">Имя компонента</param>
        /// <param name="usingOrder">Определяет очередность вызова компонента, если существует несколько компонентов, обрабатывающих один вид сообщений.</param>
        protected IncomingMessageReceiver(string name, uint? usingOrder = null) : base(name, usingOrder)
        {
        }

        /// <summary>
        /// Возвращает новые сообщения для регистрации в сервисе для дальнейшей обработки.
        /// </summary>
        /// <param name="service">Сервис обработки сообщений, в котором будут зарегистрированы новые сообщения.</param>
        /// <remarks>Дополнительные типы исключений, которые могут возникнуть во время получения сообщений, могут быть описаны в документации компонента.</remarks>
        [ApiIrreversible]
        internal protected abstract List<MessageInfo<TMessage>> OnReceive(MessageServiceBase<TAppCoreSelfReference, TMessage> service);
    }
}
