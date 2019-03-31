using System;

namespace OnUtils.Types
{
    /// <summary>
    /// </summary>
    public static class TypeHelpers
    {
        /// <summary>
        /// Пробует извлечь интерфейс <paramref name="interfaceType"/> из типа <paramref name="queryType"/>. 
        /// </summary>
        /// <returns>
        /// Возвращает null, если тип <paramref name="queryType"/> не реализует и не наследует <paramref name="interfaceType"/>.
        /// Возвращает <paramref name="interfaceType"/> в остальных случаях.
        /// </returns>
        public static Type ExtractGenericInterface(Type queryType, Type interfaceType)
        {
            if (TypeHelpers.MatchesGenericType(queryType, interfaceType))
            {
                return queryType;
            }
            Type[] interfaces = queryType.GetInterfaces();
            return TypeHelpers.MatchGenericTypeFirstOrDefault(interfaces, interfaceType);
        }

        /// <summary>
        /// Пробует извлечь универсальный (generic) тип <paramref name="genericType"/> из типа <paramref name="queryType"/>. 
        /// </summary>
        /// <returns>Возвращает первый тип в цепочке наследования, реализующий <paramref name="genericType"/>, либо null, если <paramref name="genericType"/> отсутствует в цепочке наследования.</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// class A<T> { }
        /// 
        /// class B : A<int> { }
        /// 
        /// class C : B { }
        /// 
        /// class TestClass
        /// {
        ///     static int Main()
        ///     {
        ///         var t = OnUtils.Types.TypeHelpers.ExtractGenericType(typeof(C), typeof(A<>)); // t равно typeof(B).
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static Type ExtractGenericType(Type queryType, Type genericType)
        {
            if (MatchesGenericType(queryType, genericType)) return queryType;
            var baseTypes = queryType.GetBaseTypes();
            return TypeHelpers.MatchGenericTypeFirstOrDefault(baseTypes.ToArray(), genericType);
        }

        /// <summary>
        /// Возвращает true, если <paramref name="matchType"/> является универсальным типом, на основе которого построен тип <paramref name="type"/>.
        /// </summary>
        public static bool MatchesGenericType(Type type, Type matchType)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == matchType;
        }

        private static Type MatchGenericTypeFirstOrDefault(Type[] types, Type matchType)
        {
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                if (TypeHelpers.MatchesGenericType(type, matchType))
                {
                    return type;
                }
            }
            return null;
        }


    }
}
