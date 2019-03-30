using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

/// <summary>
/// </summary>
public static class TypesExtensions
{
    /// <summary>
    /// Возвращает true, если тип <paramref name="givenType"/> является наследником generic-типа <paramref name="genericType"/>.
    /// </summary>
    public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
    {
        var interfaceTypes = givenType.GetInterfaces();

        foreach (var it in interfaceTypes)
        {
            if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                return true;
        }

        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            return true;

        Type baseType = givenType.BaseType;
        if (baseType == null) return false;

        return baseType.IsAssignableToGenericType(genericType);
    }

    /// <summary>
    /// Возвращает список всех базовых типов в цепочке наследования типа <paramref name="type"/>.
    /// </summary>
    public static List<Type> GetBaseTypes(this Type type)
    {
        if (type == null || type.BaseType == typeof(object) || !type.BaseType.IsClass) return new List<Type>();

        var baseTypes = type.BaseType.GetBaseTypes();
        baseTypes.Insert(0, type.BaseType);
        return baseTypes;
    }
}

