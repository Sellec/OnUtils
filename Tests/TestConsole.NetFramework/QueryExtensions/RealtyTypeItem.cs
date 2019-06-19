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
        IQueryable<QueryBase<TItemType>> IItemTypeQueryBuilder.BuildQuery<TItemType>(IQueryable<QueryBase<TItemType>> queryBase, UnitOfWorkBase unitOfWork)
        {
            if (!typeof(TItemType).GetInterfaces().Any(x => x == typeof(IItemBaseRealtyType))) return queryBase;

            return from sourceItem in queryBase
                   join realtyType in unitOfWork.Get<RealtyTypeItem>() on sourceItem.IdItem equals realtyType.IdRealty into realtyType_j
                   from realtyType in realtyType_j.DefaultIfEmpty()
                   select new QueryRealtyType<TItemType>()
                   {
                       PreviousQuery = sourceItem,
                       IdItem = sourceItem.IdItem,
                       IdItemType = sourceItem.IdItemType,
                       IdRealtyType = realtyType != null ? (int?)realtyType.IdRealtyType : null
                   };
        }

        void IItemTypeQueryBuilder.PrepareItem<TItemType>(QueryBase<TItemType> queryItem, ItemBase item)
        {
            if (!typeof(TItemType).GetInterfaces().Any(x => x == typeof(IItemBaseRealtyType))) return;

            Func<QueryBase<TItemType>, QueryRealtyType<TItemType>> func = null;
            func = new Func<QueryBase<TItemType>, QueryRealtyType<TItemType>>(x =>
            {
                if (x is QueryRealtyType<TItemType> xFound) return xFound;
                if (x == null) return null;
                return func(x.PreviousQuery);
            });

            var queryRealtyItem = func(queryItem);
            if (queryRealtyItem != null) ((IItemBaseRealtyType)item).IdRealtyType = queryRealtyItem.IdRealtyType;
        }
    }
}
