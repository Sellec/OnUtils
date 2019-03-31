using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnUtils.Startup
{
    /// <summary>
    /// Определяет поведение загрузчика <see cref="StartupFactory"/> во время загрузки библиотеки OnUtils. Играет роль только при установке для entry assembly.
    /// В случае, если у приложения есть entry assembly (отсутствует при запуске из unmanaged-кода, т.е. для ASP.NET MVC отсутствует) и у entry assembly задан атрибут <see cref="StartupBehaviourAttribute"/> с флагом <see cref="StartupBehaviourAttribute.IsNeedStartupFactoryAuto"/> равным False, то во время загрузки библитеки OnUtils НЕ БУДЕТ запущен инициализатор <see cref="StartupFactory"/>. В этом случае требуется ручной запуск инициализатора.
    /// Во всех остальных случаях инициализатор запускается автоматически.
    /// См. <see cref="StartupFactory"/>.
    /// См. также влияние на <see cref="LibraryEnumeratorFactory.GlobalAssemblyFilter"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class StartupBehaviourAttribute : Attribute
    {
        /// <summary>
        /// </summary>
        public StartupBehaviourAttribute(bool isNeedStartupFactoryAuto)
        {
            this.IsNeedStartupFactoryAuto = isNeedStartupFactoryAuto;
        }

        /// <summary>
        /// См. описание <see cref="StartupBehaviourAttribute"/>.
        /// </summary>
        public bool IsNeedStartupFactoryAuto
        {
            get;
            private set;
        }
    }
}
