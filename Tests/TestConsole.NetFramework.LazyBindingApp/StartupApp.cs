using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;
using System;

namespace TestConsole.LazyBinding.App
{
    public class StartupApp : IConfigureBindings<ApplicationCore>, IExecuteStart<ApplicationCore>
    {
        void IConfigureBindings<ApplicationCore>.ConfigureBindings(IBindingsCollection<ApplicationCore> bindingsCollection)
        {
            Debug.WriteLineNoLog("StartupApp.ConfigureBindings");
        }

        void IExecuteStart<ApplicationCore>.ExecuteStart(ApplicationCore core)
        {
            Debug.WriteLineNoLog("StartupApp.ExecuteStart");
        }
    }
}
