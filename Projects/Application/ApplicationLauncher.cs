namespace OnUtils.Application
{
    using Architecture.AppCore;

    /// <summary>
    /// Обеспечивает правильную очередность запуска компонентов ядра приложения. 
    /// По-умолчанию <see cref="Architecture.AppCore.AppCore{TAppCore}"/> сортирует <see cref="IAutoStart"/> сущности по алфавиту по полному имени типа, это не подходит, так как менеджер модулей должен запускаться первым.
    /// </summary>
    class ApplicationLauncher : CoreComponentBase<ApplicationCore>, IComponentSingleton<ApplicationCore>, ICritical
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
