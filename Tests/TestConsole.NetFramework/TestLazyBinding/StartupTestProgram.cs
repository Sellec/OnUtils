using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;
using System;

namespace TestConsole.TestLazyBinding
{
    using LazyBinding.App;

    class StartupTestProgram : IConfigureBindings<ApplicationCore>, IExecuteStart<ApplicationCore>
    {
        void IConfigureBindings<ApplicationCore>.ConfigureBindings(IBindingsCollection<ApplicationCore> bindingsCollection)
        {
            bindingsCollection.SetSingleton<ITestComponent1, TestComponent1>();
            Debug.WriteLineNoLog("StartupTestProgram.ConfigureBindings");
        }

        void IExecuteStart<ApplicationCore>.ExecuteStart(ApplicationCore core)
        {
            Debug.WriteLineNoLog("StartupTestProgram.ExecuteStart");
            var d = core.Get<ITestComponent1>();
        }
    }
}
