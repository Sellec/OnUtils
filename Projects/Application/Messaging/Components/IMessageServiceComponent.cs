namespace OnUtils.Application.Messaging.Components
{
    using Architecture.AppCore;
    using Architecture.ObjectPool;
    using Messages;

    /// <summary>
    /// Представляет компонент сервиса обработки сообщений определенного типа.
    /// </summary>
    /// <seealso cref="IOutcomingMessageSender{TAppCoreSelfReference, TMessage}"/>
    public interface IMessageServiceComponent<TAppCoreSelfReference, TMessage> : IPoolObjectOrdered, IComponentTransient<TAppCoreSelfReference> 
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        where TMessage : MessageBase, new()
    {
        #region Методы
        /// <summary>
        /// Инициализация компонента. При инициализации в качестве аргумента передается строка <paramref name="settings"/> с сериализованными настройками компонента.
        /// </summary>
        /// <remarks>Дополнительные типы исключений, которые могут возникнуть во время инициализации, могут быть описаны в документации компонента.</remarks>
        bool Init(string settings);
        #endregion

        #region Свойства
        /// <summary>
        /// Возвращает название компонента.
        /// </summary>
        string Name { get; }
        #endregion
    }
}
