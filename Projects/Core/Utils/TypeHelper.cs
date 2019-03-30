using System;
using System.Collections.Generic;
using System.Linq;

using System.Reflection.TraceExtensions;

namespace OnUtils.Utils
{
    /// <summary>
    /// Вспомогательные методы для работы с типами и коллекциями. Взято из System.Web.WebPages.dll
    /// </summary>
    public static class TypeHelper
    {
        /// <summary>
        /// Проверяет, является ли тип <paramref name="checkedType"/> производным от <paramref name="baseType"/>.
        /// </summary>
        /// <param name="checkedType"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static bool IsHaveBaseType(Type checkedType, Type baseType)
        {
            var tt = checkedType;

            while (true)
            {
                if (baseType.IsAssignableFrom(tt)) return true;
                if (tt.IsGenericType && tt.IsConstructedGenericType())
                {
                    var ttGeneric = tt.GetGenericTypeDefinition();
                    if (ttGeneric == baseType) return true;
                    if (IsHaveBaseType(ttGeneric, baseType)) return true;
                }

                tt = tt.BaseType;
                if (tt == typeof(object) || tt == null) break;
            }

            return false;
        }

        /// <summary>
        /// Возвращает коллекцию всех значений указанного перечисления (enum) с читабельными описаниями.
        /// Если для значения присутствует атрибут <see cref="System.ComponentModel.DataAnnotations.DisplayAttribute"/>, будет использовано значение свойства <see cref="System.ComponentModel.DataAnnotations.DisplayAttribute.Name"/>.
        /// Если для значения присутствует атрибут <see cref="System.ComponentModel.DescriptionAttribute"/>, будет использовано значение свойства <see cref="System.ComponentModel.DescriptionAttribute.Description"/>.
        /// 
        /// При наличии обоих атрибутов используется <see cref="System.ComponentModel.DataAnnotations.DisplayAttribute"/>.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static IDictionary<TEnum, string> EnumFriendlyNames<TEnum>() where TEnum : struct, IConvertible
        {
            if (!typeof(TEnum).IsEnum) throw new ArgumentException("TEnum должен быть перечислимым типом (enum).");

            var collection = new Dictionary<TEnum, string>();

            Type type = typeof(TEnum);
            foreach (var p in type.GetFields().Where(x => x.FieldType.Equals(type)))
            {
                var display = p.Name;

                var a1 = p.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>(true);
                if (a1 != null && !string.IsNullOrEmpty(a1.Name)) display = a1.Name;

                var a2 = p.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>(true);
                if (a2 != null && !string.IsNullOrEmpty(a2.Description)) display = a2.Description;

                collection.Add((TEnum)Enum.Parse(type, p.Name), display);
            }

            return collection;
        }

        /// <summary>
        /// Возвращает коллекцию свойств и их значений объекта <paramref name="value"/> в виде пар ключ:значение.
        /// </summary>
        public static IDictionary<string, object> ObjectToDictionary(object value)
        {
            var valueDictionary = new Dictionary<string, object>();
            if (value != null)
            {
                var properties = PropertyHelper.GetProperties(value);
                for (int i = 0; i < properties.Length; i++)
                {
                    var propertyHelper = properties[i];
                    valueDictionary.Add(propertyHelper.Name, propertyHelper.GetValue(value));
                }
            }
            return valueDictionary;
        }

        /// <summary>
        /// Добавляет значения свойств объекта <paramref name="value"/> к коллекции <paramref name="dictionary"/>.
        /// Для получения свойств <paramref name="value"/> используется <see cref="ObjectToDictionary(object)"/>.
        /// </summary>
        public static void AddAnonymousObjectToDictionary(IDictionary<string, object> dictionary, object value)
        {
            var routeValueDictionary = TypeHelper.ObjectToDictionary(value);
            foreach (var current in routeValueDictionary)
            {
                dictionary[current.Key] = current.Value;
            }
        }

        /// <summary>
        /// Определяет, является ли указанный тип анонимным.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsAnonymousType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (Attribute.IsDefined(type, typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false) && type.IsGenericType && type.Name.Contains("AnonymousType") && (type.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase) || type.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase)))
            {
                var arg_6D_0 = type.Attributes;
                return 0 == 0;
            }
            return false;
        }
    }

}
