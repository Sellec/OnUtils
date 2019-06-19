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
    [Table("RealtyTypeItem")]
    public class RealtyTypeItem
    {
        [Key]
        [Column(Order = 0)]
        public int IdRealty { get; set; }

        [Key]
        [Column(Order = 1)]
        public int IdRealtyType { get; set; }
    }

    public class QueryRealtyType<TItemType> : QueryBase<TItemType> where TItemType : ItemBase
    {
        public int? IdRealtyType { get; set; }
    }

    public interface IItemBaseRealtyType
    {
        int? IdRealtyType { get; set; }
    }

    public class RealtyTypeQueryBuilder : IItemTypeQueryBuilder
    {
        IQueryable<QueryResult<TItemType>> IItemTypeQueryBuilder.BuildQuery<TItemType>(IQueryable<QueryResult<TItemType>> queryBase, UnitOfWorkBase unitOfWork)
        {
            if (!typeof(TItemType).GetInterfaces().Any(x => x == typeof(IItemBaseRealtyType))) return queryBase;

            return from sourceItem in queryBase
                   join realtyType in unitOfWork.Get<RealtyTypeItem>() on sourceItem.IdItem equals realtyType.IdRealty into realtyType_j
                   from realtyType in realtyType_j.DefaultIfEmpty()
                   select new QueryResult<TItemType>()
                   {
                       Item = sourceItem.Item,
                       IdItem = sourceItem.IdItem,
                       IdItemType = sourceItem.IdItemType,
                       AdditionalQuery = new QueryRealtyType<TItemType>()
                       {
                           PreviousQuery = sourceItem.AdditionalQuery,
                           IdRealtyType = realtyType != null ? (int?)realtyType.IdRealtyType : null
                       }
                   };
        }

        void IItemTypeQueryBuilder.PrepareItem<TItemType>(QueryResult<TItemType> queryItem)
        {
            if (!typeof(TItemType).GetInterfaces().Any(x => x == typeof(IItemBaseRealtyType))) return;

            Func<QueryBase<TItemType>, QueryRealtyType<TItemType>> func = null;
            func = new Func<QueryBase<TItemType>, QueryRealtyType<TItemType>>(x =>
            {
                if (x is QueryRealtyType<TItemType> xFound) return xFound;
                if (x == null) return null;
                return func(x.PreviousQuery);
            });

            var queryRealtyItem = func(queryItem.AdditionalQuery);
            if (queryRealtyItem != null) ((IItemBaseRealtyType)queryItem.Item).IdRealtyType = queryRealtyItem.IdRealtyType;
        }
    }
}
