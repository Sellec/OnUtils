namespace OnUtils.Architecture.AppCore
{
    /// <summary>
    /// Содержит перечисление этапов запуска ядра приложения.
    /// </summary>
    public enum ApplicationStartStep
    {
        /// <summary>
        /// Подготовка списка дополнительных обработчиков привязки типов и запуска ядра.
        /// </summary>
        PrepareAssemblyStandardList = 0,

        /// <summary>
        /// Получение привязок типов - вызов <see cref="AppCore{TAppCore}.OnBindingsRequired(DI.IBindingsCollection{TAppCore})"/> и <see cref="IConfigureBindings{TAppCore}.ConfigureBindings(DI.IBindingsCollection{TAppCore})"/>.
        /// </summary>
        BindingsRequired = 1,

        /// <summary>
        /// Сохранение полученных привязок типов и вызов <see cref="AppCore{TAppCore}.OnBindingsApplied"/>
        /// </summary>
        BindingsApplying = 2,

        /// <summary>
        /// Запуск компонентов, имеющих привязку типа и наследующих <see cref="IAutoStart"/>.
        /// </summary>
        BindingsAutoStart = 3,

        /// <summary>
        /// Запуск компонента, имеющего критическое значение для работы ядра. См. описание <see cref="ICritical"/>.
        /// </summary>
        BindingsAutoStartCritical = 4,

        /// <summary>
        /// Выполнение <see cref="AppCore{TAppCore}.OnStart"/> и <see cref="IExecuteStart{TAppCore}.ExecuteStart(TAppCore)"/>.
        /// </summary>
        Start = 4,


    }
}
