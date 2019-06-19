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
    [Table("UrlTranslation")]
    public class UrlTranslation
    {
        [Key]
        public int IdTranslation { get; set; }

        public int IdTranslationType { get; set; }

        public int IdItem { get; set; }

        public int IdItemType { get; set; }

        public string UrlFull { get; set; }
    }

    public class QueryTranslation<TItemType> : QueryBase<TItemType> where TItemType : ItemBase
    {
        public string UrlFull
        {
            get => string.Empty;
            set => ((IItemBaseUrlTranslation)GetItem()).Url = Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out var uri) ? uri : null;
        }
    }

    public interface IItemBaseUrlTranslation
    {
        Uri Url { get; set; }
    }

    public class UrlTranslationQueryBuilder : IItemTypeQueryBuilder
    {
        IQueryable<QueryBase<TItemType>> IItemTypeQueryBuilder.BuildQuery<TItemType>(IQueryable<QueryBase<TItemType>> queryBase, UnitOfWorkBase unitOfWork)
        {
            if (!typeof(TItemType).GetInterfaces().Any(x => x == typeof(IItemBaseUrlTranslation))) return queryBase;

            return from sourceItem in queryBase
                   join url in unitOfWork.Get<UrlTranslation>() on new { sourceItem.IdItem, sourceItem.IdItemType, IdTranslationType = 1 } equals new { url.IdItem, url.IdItemType, url.IdTranslationType } into url_j
                   from url in url_j.DefaultIfEmpty()
                   select new QueryTranslation<TItemType>()
                   {
                       PreviousQuery = sourceItem,
                       IdItem = sourceItem.IdItem,
                       IdItemType = sourceItem.IdItemType,
                       UrlFull = url.UrlFull
                   };
        }

        void IItemTypeQueryBuilder.PrepareItem<TItemType>(QueryBase<TItemType> queryItem, ItemBase item)
        {
            if (!typeof(TItemType).GetInterfaces().Any(x => x == typeof(IItemBaseUrlTranslation))) return;

            Func<QueryBase<TItemType>, QueryTranslation<TItemType>> func = null;
            func = new Func<QueryBase<TItemType>, QueryTranslation<TItemType>>(x =>
            {
                if (x is QueryTranslation<TItemType> xFound) return xFound;
                if (x == null) return null;
                return func(x.PreviousQuery);
            });

            var queryUrlTranslation = func(queryItem);
            if (queryUrlTranslation != null) ((IItemBaseUrlTranslation)item).Url = Uri.TryCreate(queryUrlTranslation.UrlFull, UriKind.RelativeOrAbsolute, out Uri uri) ? uri : null;
        }
    }
}
