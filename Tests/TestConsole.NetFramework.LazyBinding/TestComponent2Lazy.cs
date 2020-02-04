using OnUtils.Architecture.AppCore;
using System;

namespace TestConsole.LazyBinding
{
    using App;

    class TestComponent2Lazy : CoreComponentBase<ApplicationCore>, ITestComponent2
    {
        protected override void OnStart()
        {
            Debug.WriteLineNoLog("TestComponent2Lazy.OnStart");
        }

        protected override void OnStop()
        {
        }
    }
}
