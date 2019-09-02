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
        /// <remarks>
        /// Дополнительные типы исключений, которые могут возникнуть во время получения сообщений, могут быть описаны в документации компонента.
        /// Метод не предусматривает обработку ошибок, которые могут возникнуть во время фиксации полученных сообщений. 
        /// Например, если сообщения во внешней системе были помечены как полученные во время вызова метода и после этого возникла ошибка уровня базы данных при полученных сохранении сообщений, невозможно отменить отметку "Получено" во внешней системе. 
        /// Для этого необходимо воспользоваться методами <see cref="OnBeginReceive(MessageServiceBase{TAppCoreSelfReference, TMessage})"/> / <see cref="OnEndReceive(bool, MessageInfo{TMessage}, MessageServiceBase{TAppCoreSelfReference, TMessage})"/>.
        /// </remarks>
        [ApiIrreversible]
        internal protected abstract List<MessageInfo<TMessage>> OnReceive(MessageServiceBase<TAppCoreSelfReference, TMessage> service);

        /// <summary>
        /// Вызывается для получения одного сообщения для регистрации в сервисе для дальнейшей обработки. 
        /// После регистрации сообщения вызывается <see cref="OnEndReceive(bool, MessageInfo{TMessage}, MessageServiceBase{TAppCoreSelfReference, TMessage})"/> с информацией об успешности регистрации сообщения.
        /// </summary>
        /// <param name="service">Сервис обработки сообщений, в котором будут зарегистрированы новые сообщения.</param>
        /// <returns>Возвращает информацию о полученном сообщении. Если возвращает не null, то после обработки возвращенного объекта вызывается повторно для получения следующего сообщения до тех пор, пока не вернет null.</returns>
        /// <remarks>
        /// Дополнительные типы исключений, которые могут возникнуть во время получения сообщений, могут быть описаны в документации компонента.
        /// Метод предусматривает обработку ошибок, которые могут возникнуть во время фиксации полученных сообщений. 
        /// Например, если сообщения во внешней системе были помечены как полученные во время вызова метода и после этого возникла ошибка уровня базы данных при полученных сохранении сообщений, 
        /// возможно отменить отметку "Получено" во внешней системе во время вызова <see cref="OnEndReceive(bool, MessageInfo{TMessage}, MessageServiceBase{TAppCoreSelfReference, TMessage})"/>.
        /// </remarks>
        /// <seealso cref="OnEndReceive(bool, MessageInfo{TMessage}, MessageServiceBase{TAppCoreSelfReference, TMessage})"/>
        [ApiIrreversible]
        internal protected abstract MessageInfo<TMessage> OnBeginReceive(MessageServiceBase<TAppCoreSelfReference, TMessage> service);

        /// <summary>
        /// Вызывается после получения сообщения в <see cref="OnBeginReceive(MessageServiceBase{TAppCoreSelfReference, TMessage})"/> и регистрации в сервисе для дальнейшей обработки. 
        /// </summary>
        /// <param name="service">Сервис обработки сообщений, в котором будут зарегистрированы новые сообщения.</param>
        /// <param name="isSuccess">Признак успешной регистрации в сервисе.</param>
        /// <param name="messageInfo">Информация о сообщении.</param>
        /// <returns>Если <paramref name="isSuccess"/> равен false, то возвращаемый результат ни на что не влияет. Если <paramref name="isSuccess"/> равен true, то в случае возврата false зарегистрированное сообщение удаляется.</returns>
        /// <remarks>
        /// Дополнительные типы исключений, которые могут возникнуть во время получения сообщений, могут быть описаны в документации компонента.
        /// Метод предусматривает обработку ошибок, которые могут возникнуть во время установки отметки "Получено" во внешней системе. 
        /// Например, если <paramref name="isSuccess"/> равен true (т.е. сообщение было успешно зарегистрировано) и не удалось поставить отметку о получении во внешней системе (то есть сообщение будет повторно получено в <see cref="OnBeginReceive(MessageServiceBase{TAppCoreSelfReference, TMessage})"/>),
        /// то можно вернуть false и зарегистрированное сообщение будет удалено.
        /// </remarks>
        [ApiIrreversible]
        internal protected abstract bool OnEndReceive(bool isSuccess, MessageInfo<TMessage> messageInfo, MessageServiceBase<TAppCoreSelfReference, TMessage> service);

    }
}
