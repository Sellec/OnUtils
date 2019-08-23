namespace OnUtils.Application.Messaging
{
    /// <summary>
    /// Описывает сервис отправки/приема сообщений.
    /// </summary>
    public interface IMessageService<TAppCoreSelfReference> : ServiceMonitor.IMonitoredService<TAppCoreSelfReference>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        
    }
}
