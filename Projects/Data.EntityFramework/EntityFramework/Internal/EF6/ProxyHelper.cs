using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Reflection.Emit;

using CqtExpression = System.Data.Entity.Core.Common.CommandTrees.DbExpression;

namespace OnUtils.Data.EntityFramework
{
    class ProxyHelper
    {
        private static object SyncRoot = new object();
        private static ModuleBuilder _moduleBuilder = null;

        internal static readonly byte[] ProxyAssemblyPublicKey =
        {   0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x94, 0x00, 0x00, 0x00, 0x06, 0x02, 0x00, 0x00,
                0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 0x31, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00,
                0x07, 0xD1, 0xFA, 0x57, 0xC4, 0xAE, 0xD9, 0xF0, 0xA3, 0x2E, 0x84, 0xAA, 0x0F, 0xAE, 0xFD, 0x0D,
                0xE9, 0xE8, 0xFD, 0x6A, 0xEC, 0x8F, 0x87, 0xFB, 0x03, 0x76, 0x6C, 0x83, 0x4C, 0x99, 0x92, 0x1E,
                0xB2, 0x3B, 0xE7, 0x9A, 0xD9, 0xD5, 0xDC, 0xC1, 0xDD, 0x9A, 0xD2, 0x36, 0x13, 0x21, 0x02, 0x90,
                0x0B, 0x72, 0x3C, 0xF9, 0x80, 0x95, 0x7F, 0xC4, 0xE1, 0x77, 0x10, 0x8F, 0xC6, 0x07, 0x77, 0x4F,
                0x29, 0xE8, 0x32, 0x0E, 0x92, 0xEA, 0x05, 0xEC, 0xE4, 0xE8, 0x21, 0xC0, 0xA5, 0xEF, 0xE8, 0xF1,
                0x64, 0x5C, 0x4C, 0x0C, 0x93, 0xC1, 0xAB, 0x99, 0x28, 0x5D, 0x62, 0x2C, 0xAA, 0x65, 0x2C, 0x1D,
                0xFA, 0xD6, 0x3D, 0x74, 0x5D, 0x6F, 0x2D, 0xE5, 0xF1, 0x7E, 0x5E, 0xAF, 0x0F, 0xC4, 0x96, 0x3D,
                0x26, 0x1C, 0x8A, 0x12, 0x43, 0x65, 0x18, 0x20, 0x6D, 0xC0, 0x93, 0x34, 0x4D, 0x5A, 0xD2, 0x93
            };

        private static void CreatePassThroughConstructors(TypeBuilder builder, Type baseType)
        {
            foreach (var constructor in baseType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
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

                var ctor = builder.DefineConstructor(constructor.Attributes, constructor.CallingConvention, parameterTypes, requiredCustomModifiers, optionalCustomModifiers);
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

        public static object CreateTypeFromParent(Type parentType, string newTypeName, IEnumerable<MethodInfo> methods, Func<object, object, System.Linq.Expressions.MethodCallExpression, CqtExpression> actionDelegate)
        {
            try
            {
                if (parentType == null) throw new ArgumentNullException(nameof(parentType));
                if (methods == null) throw new ArgumentNullException(nameof(methods));
                if (actionDelegate == null) throw new ArgumentNullException(nameof(actionDelegate));

                var instanceType = parentType;

                if (_moduleBuilder == null)
                {
                    var aName = new AssemblyName("Microsoft.Data.Entity.Design.VersioningFacade");

                    aName.SetPublicKey(ProxyAssemblyPublicKey);

#if NETSTANDARD2_0
                    var ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
#else
                    var ab = AppDomain.CurrentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
#endif
                    _moduleBuilder = ab.DefineDynamicModule(aName.Name);
                }

                var newTypeNameString = "EF_CallTranslatorProxy_" + newTypeName;
                if (_moduleBuilder.Assembly.GetType(newTypeNameString) != null) throw new ArgumentException("Тип с указанным именем уже зарегистрирован.", nameof(newTypeName));

                var tb = _moduleBuilder.DefineType(newTypeNameString, TypeAttributes.NotPublic, parentType);

                {
                    var fieldType = typeof(Func<object, object, System.Linq.Expressions.MethodCallExpression, CqtExpression>);

                    var method = parentType.GetMethod("Translate", BindingFlags.Instance | BindingFlags.NonPublic);

                    var field = tb.DefineField("_actionDelegate", fieldType, FieldAttributes.Public);

                    var getter = tb.DefineMethod("TranslateCustom", MethodAttributes.Private
                                                                                | MethodAttributes.HideBySig
                                                                                | MethodAttributes.NewSlot
                                                                                | MethodAttributes.Virtual
                                                                                | MethodAttributes.Final,
                                                                                CallingConventions.HasThis,
                                        method.ReturnType, method.GetParameters().Select(x => x.ParameterType).ToArray());

                    var getIL = getter.GetILGenerator();
                    getIL.Emit(OpCodes.Nop);
                    getIL.Emit(OpCodes.Ldarg_0);
                    getIL.Emit(OpCodes.Ldfld, field);
                    getIL.Emit(OpCodes.Ldarg_0);
                    getIL.Emit(OpCodes.Ldarg_1);
                    getIL.Emit(OpCodes.Ldarg_2);
                    getIL.Emit(OpCodes.Callvirt, fieldType.GetMethod("Invoke"));
                    getIL.Emit(OpCodes.Ret);

                    tb.DefineMethodOverride(getter, method);

                    CreatePassThroughConstructors(tb, parentType);

#if NETSTANDARD2_0
                    var typeddd = tb.CreateTypeInfo();
#else
                    var typeddd = tb.CreateType();
#endif
                    instanceType = typeddd;
                }

                var cons = instanceType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).ToList();

                var constructor = cons.First();// instanceType.GetConstructor(BindingFlags.NonPublic, null, new Type[] { typeof(IEnumerable<MethodInfo>) }, null);
                var obj = constructor.Invoke(new object[] { methods });

                var mm = instanceType.GetField("_actionDelegate");
                mm.SetValue(obj, actionDelegate);

                return obj;
            }
            catch (Exception ex) { Debug.WriteLine("Ошибка создания прокси-типа для {0}: {1}", parentType?.FullName, ex.Message); }

            return null;
        }

    }
}

