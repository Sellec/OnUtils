namespace OnUtils.Application.Messaging.Components
{
    interface IInternal
    {
        string SerializedSettings { set; }
        bool OnStartComponent();
    }
}
