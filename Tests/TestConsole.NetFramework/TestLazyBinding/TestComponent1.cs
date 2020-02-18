using OnUtils.Architecture.AppCore;
using System;

namespace TestConsole.TestLazyBinding
{
    using LazyBinding.App;

    class TestComponent1 : CoreComponentBase<ApplicationCore>, ITestComponent1
    {
        protected override void OnStarting()
        {
            Debug.WriteLineNoLog("TestComponent1.OnStarting");
        }

        protected override void OnStarted()
        {
            Debug.WriteLineNoLog("TestComponent1.OnStarted");
        }

        protected override void OnStop()
        {
            Debug.WriteLineNoLog("TestComponent1.OnStop");
        }
    }
}
