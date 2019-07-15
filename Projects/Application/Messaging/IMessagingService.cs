namespace OnUtils.Application.Messaging
{
    using Architecture.AppCore;
    using Architecture.ObjectPool;

    /// <summary>
    /// Описывает сервис отправки/приема сообщений.
    /// </summary>
    public interface IMessagingService<TAppCoreSelfReference> : IPoolObject, ServiceMonitor.IMonitoredService<TAppCoreSelfReference>, IComponentSingleton<TAppCoreSelfReference>, IAutoStart
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        /// <summary>
        /// Указывает, что сервис поддерживает прием сообщений.
        /// </summary>
        bool IsSupportsIncoming { get; }

        /// <summary>
        /// Указывает, что сервис поддерживает отправку сообщений.
        /// </summary>
        bool IsSupportsOutcoming { get; }

        /// <summary>
        /// Возвращает длину очереди на отправку сообщений.
        /// </summary>
        int GetOutcomingQueueLength();
    }
}
