using System.Collections.Generic;
using System.Linq;

namespace OnUtils.Items
{
    /// <summary>
    /// </summary>
    public static class ItemBaseExtension
    {
        /// <summary>
        /// Возвращает коллекцию int:string для списка элементов <see cref="IItemBase"/> на основе свойств <see cref="IItemBase.ID"/> и <see cref="IItemBase.Caption"/>.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static Dictionary<int, string> ToDictionaryBase(this IEnumerable<IItemBase> collection)
        {
            return collection.ToDictionary(x => x.ID, x => x.Caption);
        }
    }
}