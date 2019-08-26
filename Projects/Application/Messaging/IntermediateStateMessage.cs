using System;

namespace OnUtils.Application.Messaging
{
    using Messages;

    class IntermediateStateMessage<TMessageType> where TMessageType : MessageBase
    {
        internal IntermediateStateMessage(TMessageType message, DB.MessageQueue messageSource)
        {
            Message = message;
            MessageSource = messageSource;
        }

        public DB.MessageQueue MessageSource
        {
            get;
        }

        public int IdQueue
        {
            get => MessageSource.IdQueue;
        }

        public TMessageType Message
        {
            get;
        }

        public DB.MessageStateType StateType
        {
            get => MessageSource.StateType;
            set => MessageSource.StateType = value;
        }

        public string State
        {
            get => MessageSource.State;
            set => MessageSource.State = value;
        }

        public int? IdTypeComponent
        {
            get => MessageSource.IdTypeComponent;
            set => MessageSource.IdTypeComponent = value;
        }

        public DateTime DateChange
        {
            set => MessageSource.DateChange = value;
        }
    }
}
