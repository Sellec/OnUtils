using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using System.ComponentModel.DataAnnotations;

namespace OnUtils.Items
{
    /// <summary>
    /// Представляет базовую сущность и некоторый набор методов и виртуальных свойств, используемых во многих расширениях и частях движка.
    /// Поддерживает атрибут <see cref="ConstructorInitializerAttribute"/> для методов класса. 
    /// </summary>
    public interface IItemBase
    {
        #region Свойства
        /// <summary>
        /// Возвращает идентификатор объекта.
        /// </summary>
        int ID { get; set; }

        /// <summary>
        /// Возвращает название (заголовок) объекта.
        /// </summary>
        string Caption { get; set; }

        /// <summary>
        /// Возвращает и задает дату последнего изменения объекта, если поддерживается классом-потомком.
        /// </summary>
        DateTime DateChangeBase { get; set; }

        /// <summary>
        /// Владелец объекта. Может быть пустым.
        /// Привязка к владельцу важна для работы некоторых методов и некоторого функционала движка.
        /// </summary>
        object Owner { get; set; }
        #endregion
    }
}
