using System;

namespace OnUtils.Application.Messaging.Messages
{
    /// <summary>
    /// Предоставляет информацию о сообщении.
    /// </summary>
    public class MessageInfo<TMessage> where TMessage : MessageBase
    {
        private string _state = null;

        internal MessageInfo(IntermediateStateMessage<TMessage> intermediateMessage)
        {
            Message = intermediateMessage.Message;
            StateType = MessageStateType.NotHandled;
            State = intermediateMessage.State;
        }

        /// <summary>
        /// Создает новый экземпляр объекта.
        /// </summary>
        /// <param name="message">См. <see cref="Message"/>.</param>
        /// <param name="messageState">См. <see cref="StateType"/>.</param>
        /// <param name="state">См. <see cref="State"/>.</param>
        public MessageInfo(TMessage message, MessageStateType messageState = MessageStateType.NotHandled, string state = null)
        {
            Message = message;
            StateType = messageState;
            State = state;
        }

        /// <summary>
        /// Сообщение для отправки.
        /// </summary>
        public TMessage Message { get; }

        /// <summary>
        /// Состояние сообщения.
        /// </summary>
        public MessageStateType StateType { get; set; }

        /// <summary>
        /// Дополнительное состояние сообщения. 
        /// Позволяет переносить дополнительную информацию о состоянии сообщения.
        /// </summary>
        public string State
        {
            get => _state;
            set
            {
                if (!string.IsNullOrEmpty(value) && value.Length > 200) throw new ArgumentOutOfRangeException("Длина состояния не может превышать 200 символов.");
                _state = value;
            }
        }

    }

}
