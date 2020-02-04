using OnUtils.Architecture.AppCore;
using System;

namespace TestConsole.LazyBinding
{
    using App;

    class TestComponent1Lazy : CoreComponentBase<ApplicationCore>, ITestComponent1
    {
        protected override void OnStart()
        {
            Debug.WriteLineNoLog("TestComponent1Lazy.OnStart");
        }

        protected override void OnStop()
        {
        }
    }
}
