using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;
using System;

namespace TestConsole.LazyBinding
{
    using App;

    class StartupTest1 : IConfigureBindings<ApplicationCore>, IExecuteStart<ApplicationCore>
    {
        void IConfigureBindings<ApplicationCore>.ConfigureBindings(IBindingsCollection<ApplicationCore> bindingsCollection)
        {
            Debug.WriteLineNoLog("StartupTest1.ConfigureBindings");
        }

        void IExecuteStart<ApplicationCore>.ExecuteStart(ApplicationCore core)
        {
            Debug.WriteLineNoLog("StartupTest1.ExecuteStart");
        }
    }
}
