using System;
using System.Collections.Generic;

namespace OnUtils.Application.Types
{
#pragma warning disable CS1591 // todo внести комментарии.
    /// <summary>
    /// Содержимое <see cref="NestedLinkCollection"/> в уплощенной форме - без вложенной иерархии.
    /// </summary>
    public class NestedListCollectionSimplified<TAppCoreSelfReference> : System.Collections.ObjectModel.Collection<KeyValuePair<Items.ItemBase<TAppCoreSelfReference>, string>>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {

    }

    /// <summary>
    /// Содержит ссылки и группы ссылок с неограниченной вложенностью.
    /// </summary>
    public class NestedLinkCollection<TAppCoreSelfReference> : List<Items.ItemBase<TAppCoreSelfReference>>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        /// <summary>
        /// Инициализирует пустой список.
        /// </summary>
        public NestedLinkCollection()
        { }

        /// <summary>
        /// Инициализирует список со значениями из <paramref name="source"/>.
        /// </summary>
        public NestedLinkCollection(params Items.ItemBase<TAppCoreSelfReference>[] source) : base(source)
        { }

        /// <summary>
        /// Инициализирует список со значениями из <paramref name="source"/>.
        /// </summary>
        public NestedLinkCollection(IEnumerable<Items.ItemBase<TAppCoreSelfReference>> source) : base(source)
        { }

        public NestedListCollectionSimplified<TAppCoreSelfReference> GetSimplifiedHierarchy(string separator = " -> ")
        {
            var items = new NestedListCollectionSimplified<TAppCoreSelfReference>();

            Action<string, string, IEnumerable<Items.ItemBase<TAppCoreSelfReference>>> action = null;
            action = (parent, _separator, source) =>
            {
                if (source != null)
                    foreach (var item in source)
                    {
                        if (item is NestedLinkGroup<TAppCoreSelfReference>)
                        {
                            var group = item as NestedLinkGroup<TAppCoreSelfReference>;
                            items.Add(new KeyValuePair<Items.ItemBase<TAppCoreSelfReference>, string>(group.SourceItem, parent + item.Caption));
                            action(item.Caption + _separator, _separator, group.Links);
                        }
                        else items.Add(new KeyValuePair<Items.ItemBase<TAppCoreSelfReference>, string>(item, parent + item.Caption));
                    }
            };

            action("", separator, this);

            return items;
        }

        /// <summary>
        /// Возвращает список элементов, отфильтрованных при помощи пользовательского фильтра <paramref name="itemFilter"/>.
        /// </summary>
        public List<Items.ItemBase<TAppCoreSelfReference>> FindNodes(Func<Items.ItemBase<TAppCoreSelfReference>, bool> itemFilter)
        {
            if (itemFilter == null) throw new ArgumentNullException(nameof(itemFilter));

            Action<List<Items.ItemBase<TAppCoreSelfReference>>> action = null;
            var filtered = new List<Items.ItemBase<TAppCoreSelfReference>>();

            action = new Action<List<Items.ItemBase<TAppCoreSelfReference>>>(x =>
            {
                foreach (var item in x)
                {
                    if (itemFilter(item)) filtered.AddIfNotExists(item);
                    if (item is NestedLinkGroup<TAppCoreSelfReference>) action((item as NestedLinkGroup<TAppCoreSelfReference>).Links);
                }
            });

            action(this);

            return filtered;
        }
    }

    /// <summary>
    /// Коллекция ссылок, при этом сам заголовок группы тоже может быть ссылкой. Например, ссылка на категорию.
    /// </summary>
    public class NestedLinkGroup<TAppCoreSelfReference> : Items.ItemBase<TAppCoreSelfReference>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        private Items.ItemBase<TAppCoreSelfReference> _groupItem = null;

        public NestedLinkGroup(string caption, params Items.ItemBase<TAppCoreSelfReference>[] childs) : this(new NestedLinkSimple<TAppCoreSelfReference>(caption), childs)
        {
        }

        public NestedLinkGroup(Items.ItemBase<TAppCoreSelfReference> groupItem, params Items.ItemBase<TAppCoreSelfReference>[] childs)
        {
            if (groupItem == null) throw new ArgumentNullException(nameof(groupItem));
            _groupItem = groupItem;
            if (childs != null && childs.Length > 0) Links.AddRange(childs);
        }

        /// <summary>
        /// Вложенные ссылки.
        /// </summary>
        public List<Items.ItemBase<TAppCoreSelfReference>> Links { get; } = new List<Items.ItemBase<TAppCoreSelfReference>>();

        public override int ID
        {
            get => _groupItem.ID;
            set => _groupItem.ID = value;
        }

        public override string Caption
        {
            get => _groupItem.Caption;
            set => _groupItem.Caption = value; 
        }

        public override DateTime DateChangeBase
        {
            get => _groupItem.DateChangeBase;
            set => _groupItem.DateChangeBase = value; 
        }

        public override Uri Url
        {
            get => _groupItem.Url;
        }

        public Items.ItemBase<TAppCoreSelfReference> SourceItem
        {
            get => _groupItem;
        }
    }

    /// <summary>
    /// Простая ссылка.
    /// </summary>
    public class NestedLinkSimple<TAppCoreSelfReference> : Items.ItemBase<TAppCoreSelfReference>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        public NestedLinkSimple(string caption, Uri url = null)
        {
            Caption = caption;
            Url = url;
        }

        public override int ID { get; set; }

        public override string Caption { get; set; }

        public override Uri Url { get; }
    }
}



