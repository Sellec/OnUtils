using System;

namespace OnUtils.Application.Messaging.Components
{
    using Messages;

    /// <summary>
    /// Базовый класс компонента для обработки зарегистрированных входящих сообщений определенного типа.
    /// </summary>
    public abstract class IncomingMessageHandler<TAppCoreSelfReference, TMessage> : MessageServiceComponent<TAppCoreSelfReference, TMessage>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        where TMessage : MessageBase, new()
    {
        /// <summary>
        /// Создает новый экземпляр компонента.
        /// </summary>
        protected IncomingMessageHandler() : this(null, null)
        {
        }

        /// <summary>
        /// Создает новый экземпляр компонента.
        /// </summary>
        /// <param name="name">Имя компонента</param>
        /// <param name="usingOrder">Определяет очередность вызова компонента, если существует несколько компонентов, обрабатывающих один вид сообщений.</param>
        protected IncomingMessageHandler(string name, uint? usingOrder = null) : base(name, usingOrder)
        {
        }

        #region Виртуальные методы
        /// <summary>
        /// Отправляет указанное сообщение.
        /// </summary>
        /// <param name="messageInfo">Информация о сообщении, которое необходимо отправить</param>
        /// <param name="service">Сервис обработки сообщений, которому принадлежит сообщение <paramref name="messageInfo"/>.</param>
        /// <returns>Если возвращает true, то сообщение считается обработанным (см. <see cref="MessageStateType.Completed"/>).</returns>
        /// <remarks>Дополнительные типы исключений, которые могут возникнуть во время отправки сообщения, могут быть описаны в документации компонента.</remarks>
        [ApiIrreversible]
        internal protected abstract bool OnPrepare(MessageInfo<TMessage> messageInfo, MessageServiceBase<TAppCoreSelfReference, TMessage> service);
        #endregion
    }

}
