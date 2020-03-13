using System.Collections.Generic;

namespace System
{
#pragma warning disable
    /// <summary>
    /// </summary>
    public static class QueryCounterExtensions
    {
        public class QueryInfo
        {
            public string QueryText { get; set; }

            public Dictionary<string, string> Parameters { get; set; }

            public TimeSpan ExecutionTime { get; set; }
        }

        [ComponentModel.EditorBrowsable(ComponentModel.EditorBrowsableState.Never), ComponentModel.Browsable(false)]
        public static void AddQuery(string queryText, Dictionary<string, string> parameters, TimeSpan executionTime)
        {
            if (string.IsNullOrEmpty(queryText)) throw new ArgumentNullException(nameof(queryText));

            if (OnQueryExecuted != null) try { OnQueryExecuted(new QueryInfo() { QueryText = queryText, Parameters = parameters, ExecutionTime = executionTime }); } catch { }
        }

        public static event Action<QueryInfo> OnQueryExecuted;
    }
    #pragma warning restore
}