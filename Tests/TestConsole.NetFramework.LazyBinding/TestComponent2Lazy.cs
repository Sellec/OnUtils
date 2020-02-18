using OnUtils.Architecture.AppCore;
using System;

namespace TestConsole.LazyBinding
{
    using App;

    class TestComponent2Lazy : CoreComponentBase<ApplicationCore>, ITestComponent2
    {
        protected override void OnStarting()
        {
            Debug.WriteLineNoLog("TestComponent2Lazy.OnStarting");
        }

        protected override void OnStarted()
        {
            Debug.WriteLineNoLog("TestComponent2Lazy.OnStarted");
        }

        protected override void OnStop()
        {
            Debug.WriteLineNoLog("TestComponent2Lazy.OnStop");
        }
    }
}
