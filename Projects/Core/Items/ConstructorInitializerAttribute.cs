using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

using System.Reflection;

namespace OnUtils.Items
{
    /// <summary>
    /// Указывает, что метод, помеченный атрибутом, должен быть вызван в конструкторе класса. 
    /// Метод не должен иметь входных параметров и не должен возвращать значение (т.е. <see cref="Action"/>).
    /// Поддерживается в ItemBase.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class MethodMarkCallerAttribute : System.Attribute
    {
        private static ConcurrentDictionary<Type, ConcurrentDictionary<Type, List<MethodInfo>>> _metadata = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, List<MethodInfo>>>();

        /// <summary>
        /// </summary>
        public static void CallMethodsInObject<TAttributeType>(object obj) where TAttributeType : MethodMarkCallerAttribute
        {
            var objType = obj.GetType();

            _metadata.GetOrAdd(objType, (k) => new ConcurrentDictionary<Type, List<MethodInfo>>());

            var attrType = typeof(TAttributeType);
            var methods2 = _metadata[objType].GetOrAdd(attrType, (k) => PrepareAttributeMethods(new Tuple<Type, Type>(objType, typeof(TAttributeType))));

            foreach (var method in methods2)
            {
                try
                {
                    method.Invoke(obj, null);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("MethodMarkCallerAttribute.CallMethodsInObject<{0}>: {1}", attrType.FullName, (ex.InnerException ?? ex).Message);
                    throw ex.InnerException ?? ex;
                }
            }
        }

        private static List<MethodInfo> PrepareAttributeMethods(Tuple<Type, Type> types)
        {
            var objType = types.Item1;
            var attrType = types.Item2;

            var f = objType;

            var methods2 = new List<MethodInfo>();

            while (f != typeof(object))
            {
                var methods = (from p in f.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                               where Attribute.IsDefined(p, attrType)
                               select p).ToList();


                foreach (var method in methods)
                {
                    if (method.IsGenericMethod) continue;
                    if (method.GetParameters().Length > 0) continue;
                    if (method.ReturnType.Name != "Void") continue;

                    methods2.Add(method);
                }

                f = f.BaseType;
            }

            return methods2;
        }

        ///// <summary>
        ///// </summary>
        //public static void CallMethodsInObjects<TAttributeType>(IEnumerable<object> objects) where TAttributeType : MethodMarkCallerAttribute
        //{
        //    var objType = obj.GetType();
        //    var objectsByType = objects.GroupBy(x => x.GetType(), x => x);

        //    var methods2 = new List<MethodInfo>();

        //    _metadata.TryAdd(objType, new ConcurrentDictionary<Type, List<MethodInfo>>());

        //    var attrType = typeof(TAttributeType);
        //    if (!_metadata[objType].ContainsKey(attrType))
        //    {
        //        var f = objType;

        //        while (f != typeof(object))
        //        {
        //            var methods = (from p in f.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        //                           where Attribute.IsDefined(p, attrType)
        //                           select p).ToList();


        //            foreach (var method in methods)
        //            {
        //                if (method.IsGenericMethod) continue;
        //                if (method.GetParameters().Length > 0) continue;
        //                if (method.ReturnType.Name != "Void") continue;

        //                methods2.Add(method);
        //            }

        //            f = f.BaseType;
        //        }

        //        _metadata[objType].TryAdd(attrType, methods2);
        //    }

        //    foreach (var method in _metadata[objType][attrType])
        //    {
        //        try
        //        {
        //            method.Invoke(obj, null);
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.WriteLine("MethodMarkCallerAttribute.CallMethodsInObject<{0}>: {1}", attrType.FullName, ex.GetLowLevelException().Message);
        //            throw;
        //        }
        //    }
        //}
    }

    /// <summary>
    /// Описывает метод, выполняемый в конструкторе типа.
    /// </summary>
    public class ConstructorInitializerAttribute : MethodMarkCallerAttribute
    {
    }

    /// <summary>
    /// Представляет метод, выполняемый при вызове <see cref="Data.IDataContext.SaveChanges()"/>.
    /// </summary>
    public class SavedInContextEventAttribute : MethodMarkCallerAttribute
    {
    }

}
