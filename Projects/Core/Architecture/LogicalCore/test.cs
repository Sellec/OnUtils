using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraceStudio.Utils.Architecture.LogicalCore
{
#pragma warning disable CS0618, CS1591
    public interface activityTest : IActivityInstant<bool>
    {
        string test1();

        void test2(int g);
    }

    class test
    {
        public void  test1()
        {
            var req = RequestClientDispatcher.Start<activityTest>();
            var task = req.Run();
        }
    }
#pragma warning restore CS0618, CS1591
}
