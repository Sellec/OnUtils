namespace OnUtils.Application.Modules
{
    using Architecture.AppCore;

    class ModulesLoadStarter : CoreComponentBase<ApplicationCore>, IComponentSingleton<ApplicationCore>, IAutoStart
    {
        public ModulesLoadStarter()
        {
        }

        protected override void OnStart()
        {
            AppCore.GetModulesManager().StartModules();
        }

        protected override void OnStop()
        {
        }
    }
}
