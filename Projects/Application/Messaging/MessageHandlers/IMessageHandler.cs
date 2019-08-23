namespace OnUtils.Application.Messaging.MessageHandlers
{
    using Architecture.AppCore;
    using Architecture.ObjectPool;

    /// <summary>
    /// Представляет обработчик сообщений определенного типа.
    /// </summary>
    /// <seealso cref="IOutcomingMessageHandler{TAppCoreSelfReference, TMessage}"/>
    public interface IMessageHandler<TAppCoreSelfReference, TMessage> : IPoolObjectOrdered, IComponentTransient<TAppCoreSelfReference> 
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        where TMessage : MessageBase, new()
    {
        #region Методы
        /// <summary>
        /// Инициализация оюработчика. При инициализации в качестве аргумента передается строка <paramref name="settings"/> с сериализованными настройками обработчика.
        /// </summary>
        /// <remarks>Дополнительные типы исключений, которые могут возникнуть во время инициализации, могут быть описаны в документации обработчика.</remarks>
        bool Init(string settings);
        #endregion

        #region Свойства
        /// <summary>
        /// Возвращает название обработчика.
        /// </summary>
        string Name { get; }
        #endregion
    }
}
