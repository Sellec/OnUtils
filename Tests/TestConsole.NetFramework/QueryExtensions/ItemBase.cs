using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnUtils.Data;
using OnUtils.Data.EntityFramework;
using OnUtils.Data.UnitOfWork;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestConsole.QueryExtensions
{
    public abstract class ItemBase
    {
        public abstract int ID { get; set; }
    }

    public class QueryResult<TItemType> where TItemType : ItemBase
    {
        public TItemType Item { get; set; }

        public int IdItem { get; set; }

        public int IdItemType { get; set; }

        public QueryBase<TItemType> AdditionalQuery { get; set; }
    }

    public class QueryBase<TItemType> where TItemType : ItemBase
    {
        public QueryBase<TItemType> PreviousQuery { get; set; }
    }

    public interface IItemTypeQueryBuilder
    {
        IQueryable<QueryResult<TItemType>> BuildQuery<TItemType>(IQueryable<QueryResult<TItemType>> queryBase, UnitOfWorkBase unitOfWork) where TItemType : ItemBase;

        void PrepareItem<TItemType>(QueryResult<TItemType> queryItem) where TItemType : ItemBase;
    }

    public class QueryBuilder
    {
        private static IQueryable<QueryResult<TItemType>> CreateQueryItem<TItemType>(IQueryable<TItemType> queryItem, int idItemType) where TItemType : ItemBase
        {
            return queryItem.Select(x => new QueryResult<TItemType>() { Item = x, IdItem = x.ID, IdItemType = idItemType, AdditionalQuery = new QueryBase<TItemType>() });
        }

        public static List<TItemType> CreateQuery<TItemType>(IQueryable<TItemType> query, UnitOfWorkBase unitOfWork) where TItemType : ItemBase
        {
            var queryBuilders = new IItemTypeQueryBuilder[] { new UrlTranslationQueryBuilder(), new RealtyTypeQueryBuilder() };

            var idItemType = 16;

            var queryBase = CreateQueryItem(query, idItemType);

            queryBuilders.ForEach(x => queryBase = x.BuildQuery(queryBase, unitOfWork));

            var items = queryBase.ToList();

            var itemsList = items.
                Select(queryItem =>
                {
                    if (queryItem.Item != null)
                        queryBuilders.ForEach(x => x.PrepareItem(queryItem));

                    return queryItem.Item;
                }).
                Where(x => x != null).
                ToList();

            return itemsList;
        }
    }
}
