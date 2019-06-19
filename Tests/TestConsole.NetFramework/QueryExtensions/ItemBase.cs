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

    public class QueryBase<TItemType> where TItemType : ItemBase
    {
        public QueryBase<TItemType> PreviousQuery { get; set; }

        public int IdItem { get; set; }

        public int IdItemType { get; set; }
    }

    public class QueryItem<TItemType> : QueryBase<TItemType> where TItemType : ItemBase
    {
        public TItemType Item { get; set; }
    }

    public interface IItemTypeQueryBuilder
    {
        IQueryable<QueryBase<TItemType>> BuildQuery<TItemType>(IQueryable<QueryBase<TItemType>> queryBase, UnitOfWorkBase unitOfWork) where TItemType : ItemBase;

        void PrepareItem<TItemType>(QueryBase<TItemType> queryItem, ItemBase item) where TItemType : ItemBase;
    }

    public class QueryBuilder
    {
        private static IQueryable<QueryBase<TItemType>> CreateQueryItem<TItemType>(IQueryable<TItemType> queryItem, int idItemType) where TItemType : ItemBase
        {
            return queryItem.Select(x => new QueryItem<TItemType>() { Item = x, IdItem = x.ID, IdItemType = idItemType, PreviousQuery = new QueryBase<TItemType>() });
        }

        public static List<TItemType> CreateQuery<TItemType>(IQueryable<TItemType> query, UnitOfWorkBase unitOfWork) where TItemType : ItemBase
        {
            var queryBuilders = new IItemTypeQueryBuilder[] { new UrlTranslationQueryBuilder(), new RealtyTypeQueryBuilder() };

            var idItemType = 16;

            var queryBase = CreateQueryItem(query, idItemType);

            queryBuilders.ForEach(x => queryBase = x.BuildQuery(queryBase, unitOfWork));

            var items = queryBase.ToList();

            Func<QueryBase<TItemType>, QueryItem<TItemType>> func = null;
            func = new Func<QueryBase<TItemType>, QueryItem<TItemType>>(x =>
            {
                if (x is QueryItem<TItemType> xFound) return xFound;
                if (x == null) return null;
                return func(x.PreviousQuery);
            });

            var itemsList = items.
                Select(queryItem =>
                {
                    var item = func(queryItem)?.Item;
                    if (item != null)
                        queryBuilders.ForEach(x => x.PrepareItem(queryItem, item));

                    return item;
                }).
                Where(x => x != null).
                ToList();

            return itemsList;
        }
    }
}
