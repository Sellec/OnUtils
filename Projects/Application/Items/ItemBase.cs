using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnUtils.Application.Items
{
    using Modules;
    using OnUtils.Items;

    /// <summary>
    /// Базовый класс для сущностей.
    /// Предоставляет некоторый набор методов и виртуальных свойств, используемых во многих расширениях и частях движка.
    /// Некоторые же части движка работают ТОЛЬКО с объектами, унаследованными от <see cref="ItemBase"/> (например, расширение CustomFields).
    /// Поддерживает атрибут <see cref="ConstructorInitializerAttribute"/> для методов класса. 
    /// </summary>
    [Serializable]
    public abstract partial class ItemBase<TAppCoreSelfReference> : IItemBase
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        /// <summary>
        /// Беспараметрический конструктор, создающий сущность, НЕ привязанную к модулю. 
        /// Подробнее про привязку к модулю см. <see cref="Owner"/>.
        /// </summary>
        public ItemBase() : this(null)
        {
        }

        /// <summary>
        /// Конструктор, принимающий в качестве аргумента ссылку на модуль-владелец сущности.
        /// Вызов конструктора с <paramref name="owner"/> = null аналогичен вызову беспараметрического конструктора.
        /// Подробнее про привязку к модулю см. <see cref="Owner"/>.
        /// </summary>
        public ItemBase(ModuleCore<TAppCoreSelfReference> owner)
        {
            this.Owner = owner;

            MethodMarkCallerAttribute.CallMethodsInObject<ConstructorInitializerAttribute>(this);
        }

        #region Свойства
        /// <summary>
        /// Возвращает идентификатор объекта.
        /// Должен быть переопределен в класса-потомке и, для сущностей из БД, привязан к целочисленному свойству-идентификатору.
        /// </summary>
        [NotMapped]
        public abstract int ID
        {
            get;
            set;
        }

        /// <summary>
        /// Возвращает название (заголовок) объекта.
        /// Должен быть переопределен в класса-потомке. Например, для сущностей из БД, может возвращать заголовок статьи, логин или никнейм пользователя и т.п.
        /// </summary>
        [NotMapped]
        public abstract string Caption
        {
            get;
            set;
        }

        /// <summary>
        /// Возвращает и задает дату последнего изменения объекта, если поддерживается классом-потомком.
        /// По-умолчанию (без переопределения в классе-потомке) возвращает null.
        /// </summary>
        [NotMapped]
        public virtual DateTime DateChangeBase
        {
            get; set;
        }

        /// <summary>
        /// Модуль, к которому относится объект. Может быть пустым.
        /// Привязка к модулю важна для работы некоторых методов и некоторого функционала движка.
        /// Важен для работы метода <see cref="GenerateLink(bool)"/>.
        /// Может быть задан напрямую, может быть передан в качестве аргумента для конструктора, может быть автоматически определен в конструкторе для класса <see cref="ItemBase{TModuleType}"/> (см. описание класса).
        /// </summary>
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        public object Owner
        {
            get;
            set;
        }

        /// <summary>
        /// Модуль, к которому относится объект. Может быть пустым.
        /// Привязка к модулю важна для работы некоторых методов и некоторого функционала движка.
        /// Важен для работы метода <see cref="GenerateLink(bool)"/>.
        /// Может быть задан напрямую, может быть передан в качестве аргумента для конструктора, может быть автоматически определен в конструкторе для класса <see cref="ItemBase{TModuleType}"/> (см. описание класса).
        /// </summary>
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        public ModuleCore<TAppCoreSelfReference> OwnerModule
        {
            get => Owner as ModuleCore<TAppCoreSelfReference>;
        }

        #endregion

        /// <summary>
        /// Возвращает <see cref="Caption"/> при приведении к строке.
        /// </summary>
        public override string ToString()
        {
            return Caption;
        }
    }
}