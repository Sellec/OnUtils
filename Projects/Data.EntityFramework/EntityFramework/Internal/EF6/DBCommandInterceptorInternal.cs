using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;

namespace OnUtils.Data.EntityFramework.Internal
{
    class DBCommandInterceptorInternal : IDbCommandInterceptor
    {
        private ConcurrentDictionary<DbCommand, DateTime> _commandsTiming = new ConcurrentDictionary<DbCommand, DateTime>();

        private Dictionary<string, string> PrepareParameters(DbCommand command)
        {
            //return string.Join("\r\n", command.Parameters.OfType<DbParameter>().Select(x => $"{x.ParameterName} = '{x.Value}'"));
            return command.Parameters.OfType<DbParameter>().ToDictionary(x => x.ParameterName, x => $"{x.Value}");
        }

        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            var time = DateTime.Now;
            if (_commandsTiming.TryRemove(command, out time)) QueryCounterExtensions.AddQuery(command.CommandText, PrepareParameters(command), DateTime.Now - time);
        }

        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            _commandsTiming.TryAdd(command, DateTime.Now);
        }

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            var time = DateTime.Now;
            if (_commandsTiming.TryRemove(command, out time)) QueryCounterExtensions.AddQuery(command.CommandText, PrepareParameters(command), DateTime.Now - time);
        }

        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            _commandsTiming.TryAdd(command, DateTime.Now);
        }

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            var time = DateTime.Now;
            if (_commandsTiming.TryRemove(command, out time)) QueryCounterExtensions.AddQuery(command.CommandText, PrepareParameters(command), DateTime.Now - time);
        }

        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            _commandsTiming.TryAdd(command, DateTime.Now);
        }
    }
}
