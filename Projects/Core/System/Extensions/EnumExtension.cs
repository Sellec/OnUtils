using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace System
{
    /// <summary>
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// Возвращает отображаемое имя для значения типа Enum.
        /// Если для значения задан атрибут <see cref="DisplayAttribute"/>, то будет использовано его свойство <see cref="DisplayAttribute.Name"/>.
        /// В противном случае будет возвращено значение value.ToString().
        /// </summary>
        public static string DisplayName(this Enum value)
        {
            Type enumType = value.GetType();
            var enumValue = Enum.GetName(enumType, value);
            MemberInfo member = enumType.GetMember(enumValue)[0];

            var attrs = member.GetCustomAttributes(typeof(DisplayAttribute), false);
            if (attrs.Length > 0)
            {
                var outString = ((DisplayAttribute)attrs[0]).Name;

                if (((DisplayAttribute)attrs[0]).ResourceType != null)
                {
                    outString = ((DisplayAttribute)attrs[0]).GetName();
                }

                return outString;
            }

            return value.ToString();
        }
    }
}
