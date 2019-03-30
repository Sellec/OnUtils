using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Transactions;

namespace OnUtils.Data
{
    /// <summary>
    /// Вспомогательные методы для работы с транзакциями.
    /// </summary>
    public class TransactionsHelper
    {
        /// <summary>
        /// Создает и возвращает новую транзакцию для чтения данных в режиме WITH NOLOCK.
        /// </summary>
        /// <returns></returns>
        public static TransactionScope ReadUncommited()
        {
            return new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadUncommitted });
        }
    }
}
