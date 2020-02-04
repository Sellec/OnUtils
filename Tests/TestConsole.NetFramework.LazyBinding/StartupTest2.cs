using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;
using System;

namespace TestConsole.LazyBinding
{
    using App;

    class StartupTest2 : IConfigureBindingsLazy<ApplicationCore>, IExecuteStartLazy<ApplicationCore>
    {
        void IConfigureBindingsLazy<ApplicationCore>.ConfigureBindingsLazy(IBindingsCollection<ApplicationCore> bindingsCollection)
        {
            Debug.WriteLineNoLog("StartupTest2.ConfigureBindingsLazy");

            bindingsCollection.SetSingleton<ITestComponent1, TestComponent1Lazy>();
            bindingsCollection.SetSingleton<ITestComponent2, TestComponent2Lazy>();
        }

        void IExecuteStartLazy<ApplicationCore>.ExecuteStartLazy(ApplicationCore core)
        {
            Debug.WriteLineNoLog("StartupTest2.ExecuteStartLazy");
            var d = core.Get<ITestComponent2>();
        }
    }
}
