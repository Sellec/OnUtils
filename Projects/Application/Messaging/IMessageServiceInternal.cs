namespace OnUtils.Application.Messaging
{
    using Architecture.AppCore;

    interface IMessageServiceInternal<TAppCoreSelfReference> : IComponentSingleton<TAppCoreSelfReference>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        void PrepareIncoming();
        void PrepareOutcoming();
    }
}
