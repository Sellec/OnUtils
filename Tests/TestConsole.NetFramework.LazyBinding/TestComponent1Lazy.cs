using OnUtils.Architecture.AppCore;
using System;

namespace TestConsole.LazyBinding
{
    using App;

    class TestComponent1Lazy : CoreComponentBase<ApplicationCore>, ITestComponent1
    {
        protected override void OnStarting()
        {
            Debug.WriteLineNoLog("TestComponent1Lazy.OnStarting");
        }

        protected override void OnStarted()
        {
            Debug.WriteLineNoLog("TestComponent1Lazy.OnStarted");
        }

        protected override void OnStop()
        {
            Debug.WriteLineNoLog("TestComponent1Lazy.OnStop");
        }
    }
}
