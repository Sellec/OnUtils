using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraceStudio.Utils.Architecture.LogicalCore
{
    /// <summary>
    /// Базовый класс запрос на поиск операции в ядре логики.
    /// </summary>
    public class RequestBase
    {
        internal Type activityType;

        internal bool isStart = false;
        internal bool isFind = false;

        internal Guid guid;

        internal object call2 = null;

        internal RequestBase()
        {
            if (!this.GetType().IsGenericType) throw new TypeAccessException();

            var typeGeneric = this.GetType().GetGenericTypeDefinition();
            if (typeGeneric != typeof(RequestExecutePre<>) &&
                typeGeneric != typeof(RequestExecute<>) &&
                typeGeneric != typeof(RequestExecute<,>) &&
                typeGeneric != typeof(RequestStart<>) &&
                typeGeneric != typeof(RequestStart<,>)
            ) throw new TypeAccessException();
        }
    }

}
