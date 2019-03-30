using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace OnUtils.Data.EntityFramework.Internal.DB
{
    internal static class MemberInfoExtensions
    {
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public static object GetValue(this MemberInfo memberInfo)
        {
            //DebugCheck.NotNull(memberInfo);
            //Debug.Assert(memberInfo is PropertyInfo || memberInfo is FieldInfo);

            var asPropertyInfo = memberInfo as PropertyInfo;
            return asPropertyInfo != null ? asPropertyInfo.GetValue(null, null) : ((FieldInfo)memberInfo).GetValue(null);
        }

        public static MethodInfo GetDeclaredMethod(this Type type, string name, params Type[] parameterTypes)
        {
            //DebugCheck.NotNull(type);
            //DebugCheck.NotEmpty(name);
            //DebugCheck.NotNull(parameterTypes);

            return type.GetDeclaredMethods(name)
                .SingleOrDefault(m => m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));
        }

        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type, string name)
        {
            //DebugCheck.NotNull(type);
            //DebugCheck.NotEmpty(name);
#if NET40
            const BindingFlags bindingFlags
                = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            return type.GetMember(name, MemberTypes.Method, bindingFlags).OfType<MethodInfo>();
#else
            return type.GetTypeInfo().GetDeclaredMethods(name);
#endif
        }

#if NET40
        public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo memberInfo, bool inherit) where T : Attribute
        {
            //DebugCheck.NotNull(memberInfo);

            if (inherit && memberInfo.MemberType == MemberTypes.Property)
            {
                // Handle issue that .NET code doesn't honor inherit flag, but new APIs do, so we want
                // to honor it also.
                return ((PropertyInfo)memberInfo)
                    .GetPropertiesInHierarchy()
                    .SelectMany(p => p.GetCustomAttributes(typeof(T), inherit: false).OfType<T>());
            }

            return memberInfo.GetCustomAttributes(typeof(T), inherit).OfType<T>();
        }

        public static IEnumerable<PropertyInfo> GetPropertiesInHierarchy(this PropertyInfo property)
        {
            //DebugCheck.NotNull(property);

            var collection = new List<PropertyInfo> { property };
            CollectProperties(property, collection);
            return collection.Distinct();
        }

        private static void CollectProperties(PropertyInfo property, IList<PropertyInfo> collection)
        {
            //DebugCheck.NotNull(property);
            //DebugCheck.NotNull(collection);

            FindNextProperty(property, collection, getter: true);
            FindNextProperty(property, collection, getter: false);
        }

        private static void FindNextProperty(PropertyInfo property, IList<PropertyInfo> collection, bool getter)
        {
            //DebugCheck.NotNull(property);
            //DebugCheck.NotNull(collection);

            var method = getter ? property.Getter() : property.Setter();

            if (method != null)
            {
                var nextType = method.DeclaringType.BaseType();
                if (nextType != null && nextType != typeof(object))
                {
                    var baseMethod = method.GetBaseDefinition();

                    var nextProperty =
                        (from p in nextType.GetInstanceProperties()
                         let candidateMethod = getter ? p.Getter() : p.Setter()
                         where candidateMethod != null && candidateMethod.GetBaseDefinition() == baseMethod
                         select p).FirstOrDefault();

                    if (nextProperty != null)
                    {
                        collection.Add(nextProperty);
                        CollectProperties(nextProperty, collection);
                    }
                }
            }
        }

        public static IEnumerable<PropertyInfo> GetInstanceProperties(this Type type)
        {
            //DebugCheck.NotNull(type);

            return type.GetRuntimeProperties().Where(p => !p.IsStatic());
        }

#if NET40
        public static IEnumerable<PropertyInfo> GetRuntimeProperties(this Type type)
        {
            //DebugCheck.NotNull(type);

            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return type.GetProperties(bindingFlags);
        }
#endif

        public static bool IsStatic(this PropertyInfo property)
        {
            //DebugCheck.NotNull(property);

            return (property.Getter() ?? property.Setter()).IsStatic;
        }

        public static MethodInfo Getter(this PropertyInfo property)
        {
            //DebugCheck.NotNull(property);

#if NET40
            return property.GetGetMethod(nonPublic: true);
#else
            return property.GetMethod;
#endif
        }

        public static MethodInfo Setter(this PropertyInfo property)
        {
            //DebugCheck.NotNull(property);

#if NET40
            return property.GetSetMethod(nonPublic: true);
#else
            return property.SetMethod;
#endif
        }

        public static Type BaseType(this Type type)
        {
            //DebugCheck.NotNull(type);
#if NET40
            return type.BaseType;
#else
            return type.GetTypeInfo().BaseType;
#endif
        }
#endif


    }
}
