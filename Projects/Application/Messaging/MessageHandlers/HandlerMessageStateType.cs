namespace OnUtils.Application.Messaging.MessageHandlers
{
    /// <summary>
    /// Варианты состояния сообщения <see cref="HandlerMessage{TMessageType}"/> после обработки в обработчике.
    /// </summary>
    public enum HandlerMessageStateType
    {
        /// <summary>
        /// Не обработано. Сообщение будет отправлено в следующий обработчик или обработано в следующий раз, если других обработчиков нет.
        /// </summary>
        NotHandled,

        /// <summary>
        /// Полностью обработанное сообщение, с которым больше не требуется предпринимать никаких действий.
        /// </summary>
        Completed,

        /// <summary>
        /// Ошибка отправки. 
        /// Такое сообщение больше не обрабатывается, считается отправленным. 
        /// Свойство <see cref="HandlerMessage{TMessageType}.State"/> может использоваться для хранения ошибки.
        /// </summary>
        Error,

        /// <summary>
        /// Требуется повторная обработка в обработчике такого же типа. Это подходит для сообщений, которым требуется проверка состояния отправки во внешнем сервисе.
        /// </summary>
        /// <seealso cref="HandlerMessage{TMessageType}.State"/>
        RepeatWithControllerType,
    }
}
