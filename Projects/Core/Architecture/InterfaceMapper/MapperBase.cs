using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;

namespace OnUtils.Architecture.InterfaceMapper
{
    public abstract class MapperBase
    {
        private MemberInfo GetReflectedMember(MethodInfo calledMethod)
        {
            var attribute = calledMethod.GetCustomAttributes(true).OfType<InterfaceReflectedMemberAttribute>().FirstOrDefault() as InterfaceReflectedMemberAttribute;
            if (attribute != null)
            {
                var type = this.GetType().GetInterfaces().Where(x => x.FullName == attribute.MemberTypeFullName).FirstOrDefault();
                if (type != null)
                {
                    var member = type.GetMembers().Where(x => x.ToString() == attribute.MemberSignature).FirstOrDefault();
                    if (member != null) return member;
                }
            }

            return null;// attribute?.MemberSignature as MemberInfo;
        }

        /// <summary>
        /// </summary>
        internal protected object PrepareMethodCall(MethodInfo method, object[] arguments)
        {
            if (method == null)
            {
                var stack = new System.Diagnostics.StackTrace(1);
                var stackFrame = stack.GetFrame(0);
                var stackMethod = stackFrame.GetMethod();
                method = GetReflectedMember(stackMethod as MethodInfo) as MethodInfo;
            }

            if (method == null) throw new MissingMethodException("Не получилось найти выполняемый метод интерфейса.");

            var value = OnPrepareMethodCall(method, arguments);
            if (method.ReturnType == typeof(void) || value == null) return null;
            if (!method.ReturnType.IsAssignableFrom(value.GetType())) throw new InvalidCastException($"Ожидался тип {method.ReturnType.FullName} или его наследник.");

            return value;
        }

        /// <summary>
        /// Вызывается, когда необходимо обработать вызов метода прокси-класса, реализующий метод <paramref name="method"/> интерфейса. 
        /// </summary>
        /// <param name="method">Вызванный метод интерфейса.</param>
        /// <param name="arguments">Список аргументов, переданных при вызове метода интерфейса.</param>
        /// <returns>Если тип возвращаемого значения метода равен <see cref="System.Void"/>, то возвращаемое значение не играет роли и не учитывается. Во всех остальных случаях необходимо вернуть значение, которое может быть приведено к типу возвращаемого значения метода <see cref="MethodInfo.ReturnType"/>.</returns>
        protected abstract object OnPrepareMethodCall(MethodInfo method, object[] arguments);

        /// <summary>
        /// </summary>
        internal protected object PreparePropertyGet(PropertyInfo property)
        {
            if (property == null)
            {
                var stack = new System.Diagnostics.StackTrace(1);
                var stackFrame = stack.GetFrame(0);
                var stackMethod = stackFrame.GetMethod();
                property = GetReflectedMember(stackMethod as MethodInfo) as PropertyInfo;
            }

            if (property == null) throw new MissingMethodException("Не получилось найти свойство интерфейса.");

            var value = OnPreparePropertyGet(property);
            if (property.PropertyType == typeof(void) || value == null) return null;
            if (!property.PropertyType.IsAssignableFrom(value.GetType())) throw new InvalidCastException($"Ожидался тип {property.PropertyType.FullName} или его наследник.");

            return value;
        }

        /// <summary>
        /// Вызывается, когда необходимо обработать вызов метода GET для свойства прокси-класса, реализующего метод GET свойства <paramref name="property"/> интерфейса. 
        /// </summary>
        /// <param name="property">Свойство интерфейса, для которого необходимо получить значение.</param>
        /// <returns>Необходимо вернуть значение, которое может быть приведено к типу возвращаемого значения свойства <see cref="PropertyInfo.PropertyType"/>.</returns>
        protected abstract object OnPreparePropertyGet(PropertyInfo property);

        /// <summary>
        /// </summary>
        internal protected void PreparePropertySet(PropertyInfo property, object value)
        {
            if (property == null)
            {
                var stack = new System.Diagnostics.StackTrace(1);
                var stackFrame = stack.GetFrame(0);
                var stackMethod = stackFrame.GetMethod();
                property = GetReflectedMember(stackMethod as MethodInfo) as PropertyInfo;
            }

            OnPreparePropertySet(property, value);
        }

        /// <summary>
        /// Вызывается, когда необходимо обработать вызов метода SET для свойства прокси-класса, реализующего метод SET свойства <paramref name="property"/> интерфейса. 
        /// </summary>
        /// <param name="property">Свойство интерфейса, для которого необходимо задать значение.</param>
        /// <param name="value">Новое значение свойства интерфейса.</param>
        protected abstract void OnPreparePropertySet(PropertyInfo property, object value);

    }
}
