using System;
using System.Transactions;

namespace OnUtils.Data.EntityFramework.Internal
{
    using Data;

    class TransactionScopeInternal : ITransactionScope
    {
        private TransactionScope _scope = null;

        public TransactionScopeInternal()
        {
            _scope = new TransactionScope();
        }

        public TransactionScopeInternal(TransactionScopeOption scopeOption)
        {
            _scope = new TransactionScope(scopeOption);
        }

        public TransactionScopeInternal(Transaction transactionToUse)
        {
            _scope = new TransactionScope(transactionToUse);
        }

        public TransactionScopeInternal(TransactionScopeOption scopeOption, TimeSpan scopeTimeout)
        {
            _scope = new TransactionScope(scopeOption, scopeTimeout);
        }

        public TransactionScopeInternal(TransactionScopeOption scopeOption, TransactionOptions transactionOptions)
        {
            _scope = new TransactionScope(scopeOption, transactionOptions);
        }

        public TransactionScopeInternal(Transaction transactionToUse, TimeSpan scopeTimeout)
        {
            _scope = new TransactionScope(transactionToUse, scopeTimeout);
        }

        public TransactionScopeInternal(TransactionScopeOption scopeOption, TransactionOptions transactionOptions, EnterpriseServicesInteropOption interopOption)
        {
            _scope = new TransactionScope(scopeOption, transactionOptions, interopOption);
        }

        public TransactionScopeInternal(Transaction transactionToUse, TimeSpan scopeTimeout, EnterpriseServicesInteropOption interopOption)
        {
            _scope = new TransactionScope(transactionToUse, scopeTimeout, interopOption);
        }

        public void Commit()
        {
            _scope.Complete();
        }

        public void Dispose()
        {
            try
            {
                _scope.Dispose();
            }
            catch
            {
                throw;
            }
        }
    }
}
