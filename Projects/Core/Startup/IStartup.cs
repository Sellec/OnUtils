using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace OnUtils.Startup
{
    /// <summary>
    /// Представляет инициализатор, запускаемый при загрузке (Load) сборки (<see cref="Assembly"/>).
    /// Во время работы <see cref="StartupFactory.Startup"/> обнаруживаются все классы, реализующие интерфейс. Далее, если у класса есть беспараметрический публичный конструктор, создается экземпляр типа и вызывается метод <see cref="IStartup.Startup"/>.
    /// </summary>
    public interface IStartup
    {
        /// <summary>
        /// Метод, вызываемый загрузчиком инициализаторов. Вызывается один раз для каждой сборки.
        /// В силу особенностей инициализации разных видов приложений, может вызываться не моментально при загрузке.
        /// См. <see cref="StartupBehaviourAttribute"/>.
        /// </summary>
        void Startup();
    }
}
