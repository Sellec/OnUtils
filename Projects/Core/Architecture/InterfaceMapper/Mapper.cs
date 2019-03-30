using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Reflection.Emit;

using System.ComponentModel.DataAnnotations;

namespace OnUtils.Architecture.InterfaceMapper
{
    /// <summary>
    /// Предоставляет возможность генерации маппера для перехвата обращения к методам и свойствам интерфейсов. Может быть использовано, например, для генерации прозрачного прокси.
    /// </summary>
    public class Mapper
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<Type, Type> _proxyTypes = new System.Collections.Concurrent.ConcurrentDictionary<Type, Type>();

        private static void CreatePassThroughConstructors(TypeBuilder builder, Type baseType)
        {
            foreach (var constructor in baseType.GetConstructors())
            {
                var parameters = constructor.GetParameters();
                if (parameters.Length > 0 && parameters.Last().IsDefined(typeof(ParamArrayAttribute), false))
                {
                    //throw new InvalidOperationException("Variadic constructors are not supported");
                    continue;
                }

                var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();
                var requiredCustomModifiers = parameters.Select(p => p.GetRequiredCustomModifiers()).ToArray();
                var optionalCustomModifiers = parameters.Select(p => p.GetOptionalCustomModifiers()).ToArray();

                var ctor = builder.DefineConstructor(MethodAttributes.Public, constructor.CallingConvention, parameterTypes, requiredCustomModifiers, optionalCustomModifiers);
                for (var i = 0; i < parameters.Length; ++i)
                {
                    var parameter = parameters[i];
                    var parameterBuilder = ctor.DefineParameter(i + 1, parameter.Attributes, parameter.Name);
                    if (((int)parameter.Attributes & (int)ParameterAttributes.HasDefault) != 0)
                    {
                        parameterBuilder.SetConstant(parameter.RawDefaultValue);
                    }

                    foreach (var attribute in BuildCustomAttributes(parameter.GetCustomAttributesData()))
                    {
                        parameterBuilder.SetCustomAttribute(attribute);
                    }
                }

                foreach (var attribute in BuildCustomAttributes(constructor.GetCustomAttributesData()))
                {
                    ctor.SetCustomAttribute(attribute);
                }

                var emitter = ctor.GetILGenerator();
                emitter.Emit(OpCodes.Nop);

                // Load `this` and call base constructor with arguments
                emitter.Emit(OpCodes.Ldarg_0);
                for (var i = 1; i <= parameters.Length; ++i)
                {
                    emitter.Emit(OpCodes.Ldarg, i);
                }
                emitter.Emit(OpCodes.Call, constructor);

                emitter.Emit(OpCodes.Ret);
            }
        }

        private static CustomAttributeBuilder[] BuildCustomAttributes(IEnumerable<CustomAttributeData> customAttributes)
        {
            return customAttributes.Select(attribute =>
            {
                var attributeArgs = attribute.ConstructorArguments.Select(a => a.Value).ToArray();
                var namedPropertyInfos = attribute.NamedArguments.Select(a => a.MemberInfo).OfType<PropertyInfo>().ToArray();
                var namedPropertyValues = attribute.NamedArguments.Where(a => a.MemberInfo is PropertyInfo).Select(a => a.TypedValue.Value).ToArray();
                var namedFieldInfos = attribute.NamedArguments.Select(a => a.MemberInfo).OfType<FieldInfo>().ToArray();
                var namedFieldValues = attribute.NamedArguments.Where(a => a.MemberInfo is FieldInfo).Select(a => a.TypedValue.Value).ToArray();
                return new CustomAttributeBuilder(attribute.Constructor, attributeArgs, namedPropertyInfos, namedPropertyValues, namedFieldInfos, namedFieldValues);
            }).ToArray();
        }

        private static void CheckInterfaceType<TInterface>()
        {
            if (!typeof(TInterface).IsInterface) throw new ArgumentException($"Параметр-тип {nameof(TInterface)} должен быть интерфейсом.");
        }

        /// <summary>
        /// Создает
        /// </summary>
        /// <typeparam name="TMapper"></typeparam>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public static TInterface CreateObjectFromInterface<TMapper, TInterface>() where TMapper : MapperBase, new()
        {
            CheckInterfaceType<TInterface>();

            try
            {
                var instanceType = CreateTypeFromInterface<TMapper, TInterface>();
                var obj = Activator.CreateInstance(instanceType);

                return (TInterface)obj;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(CreateObjectFromInterface)}<{typeof(TInterface).Name}>: {ex.Message}");
                throw ex;
            }
        }

        /// <summary>
        /// Генерирует новый или возвращает ранее сконструированный тип на базе типа <typeparamref name="TMapper"/>, наследующий и реализующий методы интерфейса <typeparamref name="TInterface"/>. 
        /// Обращение к свойствам и методам объекта через интерфейса на основе сконструированного типа должно обрабатываться в <see cref="MapperBase.OnPrepareMethodCall(MethodInfo, object[])"/> / <see cref="MapperBase.OnPreparePropertyGet(PropertyInfo)"/> / <see cref="MapperBase.OnPreparePropertySet(PropertyInfo, object)"/>.
        /// <para>После создания сконструированный тип сопоставляется паре <typeparamref name="TMapper"/>-<typeparamref name="TInterface"/> и повторно возвращается при последующих вызовах данной пары.</para>
        /// </summary>
        public static Type CreateTypeFromInterface<TMapper, TInterface>() where TMapper : MapperBase
        {
            var type = typeof(TInterface);
            var instanceType = _proxyTypes.GetOrAdd(type, (key) => CreateTypeFromParentInternal<TMapper, TInterface>());
            return instanceType;
        }

        private static Type CreateTypeFromParentInternal<TMapper, TInterface>() where TMapper : MapperBase
        {
            try
            {
                var parentType = typeof(TMapper);
                var instanceType = parentType;

                var nameSuffix = Guid.NewGuid().ToString();
                //nameSuffix = "1";

                var aName = new AssemblyName("InterfaceMapper_" + nameSuffix);
#if NETSTANDARD2_0
                var ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
                var mb = ab.DefineDynamicModule(aName.Name);
#else
                var ab = AppDomain.CurrentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndSave, "D:\\");
                var mb = ab.DefineDynamicModule(aName.Name, aName.Name + ".dll");
#endif

                var tb = mb.DefineType("TransparentProxy_" + Guid.NewGuid().ToString(), TypeAttributes.Public, parentType, new Type[] { typeof(TInterface) });

                var prepareMethodCallMethod = parentType.GetMethod(nameof(MapperBase.PrepareMethodCall), BindingFlags.Instance | BindingFlags.NonPublic);
                var preparePropertyGetMethod = parentType.GetMethod(nameof(MapperBase.PreparePropertyGet), BindingFlags.Instance | BindingFlags.NonPublic);
                var preparePropertySetMethod = parentType.GetMethod(nameof(MapperBase.PreparePropertySet), BindingFlags.Instance | BindingFlags.NonPublic);

                if (prepareMethodCallMethod != null)
                {
                    CreatePassThroughConstructors(tb, parentType);

                    //Реализуем методы интерфейса.
                    foreach (var method in typeof(TInterface).GetMethods())
                    {
                        if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))) continue;

                        var parameters = method.GetParameters();

                        var methodNew = tb.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual, method.ReturnType, parameters.Select(x => x.ParameterType).ToArray());

                        //var reflectedAttribute = typeof(InterfaceReflectedMemberAttribute).GetConstructor(new Type[] { typeof(string) });
                        //methodNew.SetCustomAttribute(new CustomAttributeBuilder(reflectedAttribute, new object[] { method.ToString() }));

                        var reflectedAttribute = typeof(InterfaceReflectedMemberAttribute).GetConstructor(new Type[] { typeof(string), typeof(string) });
                        methodNew.SetCustomAttribute(new CustomAttributeBuilder(reflectedAttribute, new object[] { typeof(TInterface).FullName, method.ToString() }));

                        var methodNewIL = methodNew.GetILGenerator();

                        methodNewIL.DeclareLocal(typeof(object[]));

                        methodNewIL.Emit(OpCodes.Ldc_I4, parameters.Length);
                        methodNewIL.Emit(OpCodes.Newarr, typeof(object));

                        methodNewIL.Emit(OpCodes.Stloc_0);

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            methodNewIL.Emit(OpCodes.Ldloc_0);
                            methodNewIL.Emit(OpCodes.Ldc_I4, i);
                            methodNewIL.Emit(OpCodes.Ldc_I4, 10);
                            if (parameters[i].ParameterType.IsValueType) methodNewIL.Emit(OpCodes.Box, typeof(object));
                            methodNewIL.Emit(OpCodes.Stelem_Ref);
                        }

                        methodNewIL.Emit(OpCodes.Ldarg_0);
                        methodNewIL.Emit(OpCodes.Ldnull);
                        methodNewIL.Emit(OpCodes.Ldloc_0);

                        methodNewIL.Emit(OpCodes.Call, prepareMethodCallMethod);
                        methodNewIL.Emit(OpCodes.Pop);
                        methodNewIL.Emit(OpCodes.Ret);
                    }

                    // Реализуем свойства интерфейса.
                    foreach (var property in typeof(TInterface).GetProperties())
                    {
                        var propertyNew = tb.DefineProperty(property.Name, PropertyAttributes.None, property.PropertyType, null);

                        // Методы GET-SET
                        var getSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;

                        if (property.CanRead)
                        {
                            var getter = tb.DefineMethod("get_" + property.Name, getSetAttr, property.PropertyType, Type.EmptyTypes);

                            var reflectedAttribute = typeof(InterfaceReflectedMemberAttribute).GetConstructor(new Type[] { typeof(string), typeof(string) });
                            getter.SetCustomAttribute(new CustomAttributeBuilder(reflectedAttribute, new object[] { typeof(TInterface).FullName, property.ToString() }));

                            var getIL = getter.GetILGenerator();
                            getIL.Emit(OpCodes.Ldarg_0);
                            getIL.Emit(OpCodes.Ldnull);
                            getIL.Emit(OpCodes.Call, preparePropertyGetMethod);
                            if (property.PropertyType.IsValueType)
                            {
                                getIL.Emit(OpCodes.Unbox_Any, property.PropertyType);
                            }
                            getIL.Emit(OpCodes.Ret);
                            propertyNew.SetGetMethod(getter);

                            tb.DefineMethodOverride(propertyNew.GetGetMethod(), property.GetGetMethod());
                        }

                        if (property.CanWrite)
                        {
                            var setter = tb.DefineMethod("set_" + property.Name, getSetAttr, null, new Type[] { property.PropertyType });

                            var reflectedAttribute = typeof(InterfaceReflectedMemberAttribute).GetConstructor(new Type[] { typeof(string), typeof(string) });
                            setter.SetCustomAttribute(new CustomAttributeBuilder(reflectedAttribute, new object[] { typeof(TInterface).FullName, property.ToString() }));

                            var setIL = setter.GetILGenerator();


                            setIL.Emit(OpCodes.Ldarg_0);
                            setIL.Emit(OpCodes.Ldnull);
                            if (property.PropertyType.IsValueType)
                            {
                                setIL.Emit(OpCodes.Ldarg_1);
                                setIL.Emit(OpCodes.Box, property.PropertyType);
                            }

                            setIL.Emit(OpCodes.Call, preparePropertySetMethod);
                            setIL.Emit(OpCodes.Ret);

                            propertyNew.SetSetMethod(setter);

                            tb.DefineMethodOverride(propertyNew.GetSetMethod(), property.GetSetMethod());
                        }
                    }

#if NETSTANDARD2_0
                    var typeddd = tb.CreateTypeInfo();
#else
                    var typeddd = tb.CreateType();
#endif
                    instanceType = typeddd;
                }

                //ab.Save("1.dll");

                return instanceType;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(CreateTypeFromInterface)}<{typeof(TInterface).Name}>: {ex.Message}");
                throw ex;
            }
        }

    }
}
