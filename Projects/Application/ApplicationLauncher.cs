namespace OnUtils.Application
{
    using Architecture.AppCore;

    /// <summary>
    /// Обеспечивает правильную очередность запуска компонентов ядра приложения. 
    /// По-умолчанию <see cref="Architecture.AppCore.AppCore{TAppCore}"/> сортирует <see cref="IAutoStart"/> сущности по алфавиту по полному имени типа, это не подходит, так как менеджер модулей должен запускаться первым.
    /// </summary>
    class ApplicationLauncher<TAppCoreSelfReference> : CoreComponentBase<TAppCoreSelfReference>, IComponentSingleton<TAppCoreSelfReference>, ICritical
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        protected override void OnStart()
        {
            var modulesManager = AppCore.GetModulesManager();
        }

        protected override void OnStop()
        {
        }
    }
}
