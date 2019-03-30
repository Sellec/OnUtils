using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#pragma warning disable CS1591
namespace OnUtils.Architecture.FOM.SampleModel
{
    /// <summary>
    /// Пример модели логики. Паттерн FOM (Factory-Object-Manipulator) подразумевает сведение всей бизнес-логики к блокам "Фабрика объектов - объект - манипулятор объектом".
    /// <para>Фабрика объектов - фабрика отвечает за создание новых и получение готовых объектов определенных типов (см. <see cref="IObjectFactory{T}"/>). Может существовать семейство фабрик объектов определенного типа, в зависимости от реализации логики внутри блока.</para>
    /// <para>Объект - объект модели, хранящий данные, но не выполняющий самостоятельно никаких действий.</para>
    /// <para>Манипулятор - манипулятор выполняет все действия с объектом определенного типа (см. <see cref="IObjectManipulator{T}"/>), подразумеваемые логикой модели. Может существовать семейство манипуляторов объектами определенного типа, в зависимости от реализации логики внутри блока.</para>
    /// <para>Блоки FOM необязательно должны быть сведены к классам типа <see cref="SampleModel"/>. 
    /// В зависимости от реализации, это могут быть пулы всех фабрик и всех манипуляторов, либо модули в модульной архитектуре. 
    /// Часть логики может существовать внутри классов типа <see cref="SampleModel"/> (как в примере <see cref="Example1.Run"/>), так и вынесена вовне (как в примере <see cref="Example2.Run"/>, в зависимости от организации архитектуры приложения.</para>
    /// <para>Блоки FOM могут включать в себя другие блоки или объекты из блоков и манипулировать ими с помощью соответствующих манипуляторов. Логика строится на вложенности блоков и манипулировании объектами нижнего уровня.</para>
    /// </summary>
    public class SampleModel
    {
        private List<object> _instances = new List<object>();

        /// <summary>
        /// Создает новый или возвращает существующий экземпляр фабрики объектов <see cref="SampleObject"/>.
        /// </summary>
        public ISampleObjectFactory CreateFactory()
        {
            var factory = _instances.OfType<ISampleObjectFactory>().FirstOrDefault();
            if (factory != null) return factory;

            factory = new SampleObjectFactory();
            _instances.Add(factory);
            return factory;
        }

        /// <summary>
        /// Создает новый или возвращает существующий экземпляр манипулятора объектами <see cref="SampleObject"/>.
        /// </summary>
        public ISampleObjectManipulator CreateManipulator()
        {
            var manipulator = _instances.OfType<ISampleObjectManipulator>().FirstOrDefault();
            if (manipulator != null) return manipulator;

            manipulator = new SampleObjectManipulator();
            _instances.Add(manipulator);
            return manipulator;
        }

        /// <summary>
        /// Создает новый экземпляр объекта <see cref="SampleObject"/> и выводит информацию в логи.
        /// </summary>
        public void WriteIntoLogs()
        {
            var obj = CreateFactory().CreateEmpty();
            CreateManipulator().WriteIntoLogs(obj);
        }

    }

    public class Example1
    {
        public void Run()
        {
            var model = new SampleModel();
            model.WriteIntoLogs();
        }
    }

    public class Example2
    {
        public void Run()
        {
            var factory = new SampleObjectFactory();
            var manipulator = new SampleObjectManipulator();

            var obj = (factory as ISampleObjectFactory).CreateEmpty();
            (manipulator as ISampleObjectManipulator).WriteIntoLogs(obj);
        }
    }
}
#pragma warning restore CS1591
