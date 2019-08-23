using System;

namespace OnUtils.Application.Messaging.MessageHandlers
{
    /// <summary>
    /// Предоставляет обработчику информацию о сообщении.
    /// </summary>
    public class HandlerMessage<TMessageType> where TMessageType : MessageBase
    {
        private string _state = null;

        internal HandlerMessage(IntermediateStateMessage<TMessageType> intermediateMessage)
        {
            MessageBody = intermediateMessage.Message;
            HandledState = HandlerMessageStateType.NotHandled;
            State = intermediateMessage.State;
        }

        /// <summary>
        /// Если обработчик установил значение, отличное от <see cref="HandlerMessageStateType.NotHandled"/>, то дальнейшая обработка сообщения прекращается.
        /// </summary>
        public HandlerMessageStateType HandledState { get; set; }

        /// <summary>
        /// Сообщение для отправки.
        /// </summary>
        public TMessageType MessageBody { get; }

        /// <summary>
        /// Состояние сообщения на момент передачи в обработчик.
        /// </summary>
        public MessageStateType StateType { get; }

        /// <summary>
        /// Дополнительное состояние сообщения. 
        /// Может использоваться в обработчиках вместе с <see cref="MessageStateType.RepeatWithControllerType"/> для отслеживания состояния отправки во внешних сервисах.
        /// Если <see cref="HandledState"/> равно <see cref="HandlerMessageStateType.RepeatWithControllerType"/> или <see cref="HandlerMessageStateType.Error"/>, то значение свойства записывается для дальнейшего использования.
        /// Если <see cref="HandledState"/> равно <see cref="HandlerMessageStateType.Completed"/>, то значение свойства сбрасывается, так как оно больше не несет пользы.
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
