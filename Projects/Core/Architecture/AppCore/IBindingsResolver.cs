namespace OnUtils.Architecture.AppCore
{
    using DI;

    /// <summary>
    /// Предоставляет возможность разрешать отсутствующую привязку типов.
    /// </summary>
    /// <seealso cref="AppCore{TAppCore}.GetBindingsResolver"/>
    /// <seealso cref="AppCore{TAppCore}.SetBindingsResolver(IBindingsResolver{TAppCore})"/>
    public interface IBindingsResolver<TAppCore>
    {
        /// <summary>
        /// Вызывается, когда необходимо разрешить привязку query-типа <typeparamref name="TRequestedType"/> для ядра <typeparamref name="TAppCore"/>.
        /// </summary>
        void OnSingletonBindingResolve<TRequestedType>(ISingletonBindingsHandler<TAppCore> bindingsHandler)
            where TRequestedType : IComponentSingleton<TAppCore>;

        /// <summary>
        /// Вызывается, когда необходимо разрешить привязку query-типа <typeparamref name="TRequestedType"/> для ядра <typeparamref name="TAppCore"/>.
        /// </summary>
        void OnTransientBindingResolve<TRequestedType>(ITransientBindingsHandler<TAppCore> bindingsHandler)
            where TRequestedType : IComponentTransient<TAppCore>;
    }
}
