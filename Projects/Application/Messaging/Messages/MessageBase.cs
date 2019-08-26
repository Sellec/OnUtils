namespace OnUtils.Application.Messaging.Messages
{
    /// <summary>
    /// Базовый класс сообщения. Все специфические типы сообщений сервисов должны наследоваться от него.
    /// </summary>
    public abstract class MessageBase
    {
        /// <summary>
        /// Тема сообщения.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Тело сообщения.
        /// </summary>
        public object Body { get; set; }
    }
}
