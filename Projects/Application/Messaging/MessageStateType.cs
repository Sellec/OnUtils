namespace OnUtils.Application.Messaging
{
    /// <summary>
    /// Варианты состояния сообщения.
    /// </summary>
    public enum MessageStateType : byte
    {
        /// <summary>
        /// Не обработано. Такое сообщение считывается в <see cref="MessageServiceBase{TAppCoreSelfReference, TMessageType}.GetUnsentMessages"/> и обрабатывается соответствующими обработчиками.
        /// </summary>
        NotProcessed = 0,

        /// <summary>
        /// Обработка завершена.
        /// </summary>
        Complete = 1,

        /// <summary>
        /// Ошибка обработки. Такое сообщение больше не обрабатывается, считается завершенным. Свойство <see cref="DB.MessageQueue.State"/> будет содержать суть ошибки.
        /// </summary>
        Error = 2,

        /// <summary>
        /// Требуется повторная обработка в обработчике такого же типа. Это подходит для сообщений, которым требуется проверка состояния отправки во внешнем сервисе.
        /// </summary>
        /// <seealso cref="IntermediateStateMessage{TMessageType}.State"/>
        RepeatWithControllerType = 4,
    }
}
