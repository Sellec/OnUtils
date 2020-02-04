using OnUtils.Architecture.AppCore;
using System;

namespace TestConsole.TestLazyBinding
{
    using LazyBinding.App;

    class TestComponent1 : CoreComponentBase<ApplicationCore>, ITestComponent1
    {
        protected override void OnStart()
        {
            Debug.WriteLineNoLog("TestComponent1.OnStart");
        }

        protected override void OnStop()
        {
        }
    }
}
