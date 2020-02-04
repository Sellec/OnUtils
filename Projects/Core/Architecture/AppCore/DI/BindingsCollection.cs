using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OnUtils.Architecture.AppCore.DI
{
    /// <summary>
    /// Позволяет управлять привязкой классов. 
    /// </summary>
    public interface IBindingsCollection<in TAppCore> : ISingletonBindingsHandler<TAppCore>, ITransientBindingsHandler<TAppCore>
    {
        /// <summary>
        /// Регистрирует новый обработчик, вызываемый при активации нового экземпляра объекта.
        /// </summary>
        void RegisterBindingConstraintHandler(IBindingConstraintHandler handler);
    }

    /// <summary>
    /// Позволяет управлять привязкой классов. 
    /// </summary>
    public class BindingsCollection<TAppCore> : IBindingsCollection<TAppCore>
    {
        internal readonly ConcurrentDictionary<Type, BindingDescription> _typesCollection = new ConcurrentDictionary<Type, BindingDescription>();
        private List<IBindingConstraintHandler> _constraintHandlers = new List<IBindingConstraintHandler>();

        /// <summary>
        /// Регистрирует новый обработчик, вызываемый при активации нового экземпляра объекта.
        /// </summary>
        public void RegisterBindingConstraintHandler(IBindingConstraintHandler handler)
        {
            if (!_constraintHandlers.Contains(handler)) _constraintHandlers.Add(handler);
        }

        #region Задание реализаций
        #region Singleton
        /// <summary>
        /// Устанавливает привязку типа <typeparamref name="TQuery"/>. Тип <typeparamref name="TQuery"/> используется в качестве queryType и implementationType, см. описание <see cref="SetSingleton{TQuery, TImplementation}()"/>.
        /// Установленная привязка используется при создании экземпляра singleton-объекта, существующего весь цикл жизни ядра.
        /// Если ранее уже была задана привязка к <typeparamref name="TQuery"/>, то новая привязка заменяет старую.
        /// </summary>
        /// <typeparam name="TQuery">Тип, запрашиваемый при создании экземпляра объекта и используемый при создании экземпляра объекта.</typeparam>
        /// <exception cref="ArgumentException">Возникает, если <typeparamref name="TQuery"/> обозначает абстрактный класс.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если обработчик привязок <see cref="IBindingConstraintHandler"/> (см. <see cref="RegisterBindingConstraintHandler(IBindingConstraintHandler)"/>) возвращает ошибку.</exception>
        public void SetSingleton<TQuery>()
            where TQuery : class, IComponentSingleton<TAppCore>
        {
            if (typeof(TQuery).IsAbstract) throw new ArgumentException($"Параметр-тип {nameof(TQuery)} не должен быть абстрактным типом.", nameof(TQuery));
            SetSingleton(typeof(TQuery), typeof(TQuery));
        }

        /// <summary>
        /// Устанавливает привязку типа <typeparamref name="TQuery"/>. Тип <typeparamref name="TQuery"/> используется в качестве queryType и implementationType, см. описание <see cref="SetSingleton{TQuery, TImplementation}(Func{TImplementation})"/>.
        /// Установленная привязка используется при создании экземпляра singleton-объекта, существующего весь цикл жизни ядра.
        /// Если ранее уже была задана привязка к <typeparamref name="TQuery"/>, то новая привязка заменяет старую.
        /// </summary>
        /// <typeparam name="TQuery">Тип, запрашиваемый при создании экземпляра объекта и используемый при создании экземпляра объекта.</typeparam>
        /// <param name="factoryLambda">Фабрика, используемая при получении экземпляра объекта.</param>
        /// <exception cref="ArgumentException">Возникает, если <typeparamref name="TQuery"/> обозначает абстрактный класс.</exception>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="factoryLambda"/> равен null.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если обработчик привязок <see cref="IBindingConstraintHandler"/> (см. <see cref="RegisterBindingConstraintHandler(IBindingConstraintHandler)"/>) возвращает ошибку.</exception>
        public void SetSingleton<TQuery>(Func<TQuery> factoryLambda)
            where TQuery : class, IComponentSingleton<TAppCore>
        {
            if (typeof(TQuery).IsAbstract) throw new ArgumentException($"Параметр-тип {nameof(TQuery)} не должен быть абстрактным типом.", nameof(TQuery));
            SetSingleton<TQuery, TQuery>(factoryLambda);
        }

        /// <summary>
        /// Устанавливает привязку типа <typeparamref name="TQuery"/> к <typeparamref name="TImplementation"/>.
        /// Установленная привязка используется при создании экземпляра singleton-объекта, существующего весь цикл жизни ядра.
        /// Если ранее уже была задана привязка к <typeparamref name="TQuery"/>, то новая привязка заменяет старую.
        /// </summary>
        /// <typeparam name="TQuery">Тип, запрашиваемый при создании экземпляра объекта.</typeparam>
        /// <typeparam name="TImplementation">Тип, используемый при создании экземпляра объекта, приводимого к <typeparamref name="TQuery"/>.</typeparam>
        /// <exception cref="ArgumentException">Возникает, если <typeparamref name="TImplementation"/> обозначает абстрактный класс.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если обработчик привязок <see cref="IBindingConstraintHandler"/> (см. <see cref="RegisterBindingConstraintHandler(IBindingConstraintHandler)"/>) возвращает ошибку.</exception>
        public void SetSingleton<TQuery, TImplementation>()
            where TQuery : IComponentSingleton<TAppCore>
            where TImplementation : TQuery
        {
            if (typeof(TImplementation).IsAbstract) throw new ArgumentException($"Параметр-тип {nameof(TImplementation)} не должен быть абстрактным типом.", nameof(TImplementation));
            SetSingleton(typeof(TQuery), typeof(TImplementation));
        }

        /// <summary>
        /// Устанавливает привязку типа <typeparamref name="TQuery"/> к <typeparamref name="TImplementation"/>.
        /// Установленная привязка используется при создании экземпляра singleton-объекта, существующего весь цикл жизни ядра.
        /// Если ранее уже была задана привязка к <typeparamref name="TQuery"/>, то новая привязка заменяет старую.
        /// </summary>
        /// <typeparam name="TQuery">Тип, запрашиваемый при создании экземпляра объекта.</typeparam>
        /// <typeparam name="TImplementation">Тип, используемый при создании экземпляра объекта, приводимого к <typeparamref name="TQuery"/>.</typeparam>
        /// <param name="factoryLambda">Фабрика, используемая при получении экземпляра объекта.</param>
        /// <exception cref="ArgumentException">Возникает, если <typeparamref name="TImplementation"/> обозначает абстрактный класс.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если обработчик привязок <see cref="IBindingConstraintHandler"/> (см. <see cref="RegisterBindingConstraintHandler(IBindingConstraintHandler)"/>) возвращает ошибку.</exception>
        public void SetSingleton<TQuery, TImplementation>(Func<TImplementation> factoryLambda)
            where TQuery : IComponentSingleton<TAppCore>
            where TImplementation : TQuery
        {
            if (typeof(TImplementation).IsAbstract) throw new ArgumentException($"Параметр-тип {nameof(TImplementation)} не должен быть абстрактным типом.", nameof(TImplementation));
            if (factoryLambda == null) throw new ArgumentNullException(nameof(factoryLambda));
            SetSingleton(typeof(TQuery), typeof(TImplementation), () => factoryLambda());
        }

        /// <summary>
        /// Устанавливает привязку типа <paramref name="queryType"/> к <paramref name="implementationType"/>.
        /// Установленная привязка используется при создании экземпляра singleton-объекта, существующего весь цикл жизни ядра.
        /// Если ранее уже была задана привязка к <paramref name="queryType"/>, то новая привязка заменяет старую.
        /// </summary>
        /// <param name="queryType">Тип, запрашиваемый при создании экземпляра объекта.</param>
        /// <param name="implementationType">Тип, используемый при создании экземпляра объекта, приводимого к <paramref name="queryType"/>.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="queryType"/> равен null.</exception>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="implementationType"/> равен null.</exception>
        /// <exception cref="ArgumentException">Возникает, если <paramref name="queryType"/> не удовлетворяет требованиям TSingleton для метода <see cref="SetSingleton{TQuery, TImplementation}()"/>.</exception>
        /// <exception cref="ArgumentException">Возникает, если <paramref name="implementationType"/> не удовлетворяет требованиям TImplementation для метода <see cref="SetSingleton{TSingleton, TImplementation}()"/>.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если обработчик привязок <see cref="IBindingConstraintHandler"/> (см. <see cref="RegisterBindingConstraintHandler(IBindingConstraintHandler)"/>) возвращает ошибку.</exception>
        public void SetSingleton(Type queryType, Type implementationType)
        {
            SetSingleton(queryType, implementationType, CreateActivator(implementationType));
        }

        private void SetSingleton(Type queryType, Type implementationType, Func<object> creationLambda)
        {
            if (queryType == null) throw new ArgumentNullException(nameof(queryType));
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));

            if (!typeof(IComponentSingleton<TAppCore>).IsAssignableFrom(queryType)) throw new ArgumentException($"Параметр-тип {nameof(queryType)} должен наследовать {typeof(IComponentSingleton<TAppCore>).FullName}.", nameof(queryType));
            if (implementationType.IsAbstract) throw new ArgumentException($"Параметр-тип {nameof(implementationType)} не должен быть абстрактным типом.", nameof(implementationType));
            if (!queryType.IsAssignableFrom(implementationType)) throw new ArgumentException($"Параметр-тип {nameof(implementationType)} должен наследовать {nameof(queryType)}.", nameof(implementationType));

            CheckBinding(queryType, implementationType);
            _typesCollection[queryType] = new BindingDescription(implementationType, creationLambda) { Instances = null };
        }
        #endregion

        #region Transient
        /// <summary>
        /// Устанавливает привязку типа <typeparamref name="TQuery"/>. Тип <typeparamref name="TQuery"/> используется в качестве queryType и implementationType, см. описание <see cref="SetTransient{TQuery, TImplementation}"/>.
        /// Установленная привязка используется при создании экземпляров объектов, цикл жизни которых не отслеживается.
        /// Если ранее уже были заданы привязки к <typeparamref name="TQuery"/>, то новая привязка заменяет старые.
        /// </summary>
        /// <typeparam name="TQuery">Тип, запрашиваемый при создании экземпляра объекта и используемый при создании экземпляра объекта.</typeparam>
        /// <exception cref="ArgumentException">Возникает, если <typeparamref name="TQuery"/> обозначает абстрактный класс.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если обработчик привязок <see cref="IBindingConstraintHandler"/> (см. <see cref="RegisterBindingConstraintHandler(IBindingConstraintHandler)"/>) возвращает ошибку.</exception>
        public void SetTransient<TQuery>()
            where TQuery : class, IComponentTransient<TAppCore>
        {
            if (typeof(TQuery).IsAbstract) throw new ArgumentException($"Параметр-тип {nameof(TQuery)} не должен быть абстрактным типом.", nameof(TQuery));
            SetTransient<TQuery, TQuery>();
        }

        /// <summary>
        /// Устанавливает привязку типа <typeparamref name="TQuery"/> к <typeparamref name="TImplementation"/>.
        /// Установленная привязка используется при создании экземпляров объектов, цикл жизни которых не отслеживается.
        /// Если ранее уже были заданы привязки к <typeparamref name="TQuery"/>, то новая привязка заменяет старые.
        /// </summary>
        /// <typeparam name="TQuery">Тип, запрашиваемый при создании экземпляра объекта.</typeparam>
        /// <typeparam name="TImplementation">Тип, используемый при создании экземпляра объекта, приводимого к <typeparamref name="TQuery"/>.</typeparam>
        /// <exception cref="ArgumentException">Возникает, если <typeparamref name="TImplementation"/> обозначает абстрактный класс.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если обработчик привязок <see cref="IBindingConstraintHandler"/> (см. <see cref="RegisterBindingConstraintHandler(IBindingConstraintHandler)"/>) возвращает ошибку.</exception>
        public void SetTransient<TQuery, TImplementation>()
            where TQuery : IComponentTransient<TAppCore>
            where TImplementation : TQuery
        {
            if (typeof(TImplementation).IsAbstract) throw new ArgumentException($"Параметр-тип {nameof(TImplementation)} не должен быть абстрактным типом.", nameof(TImplementation));
            SetTransient<TQuery>(typeof(TImplementation));
        }

        /// <summary>
        /// Устанавливает привязки типа <typeparamref name="TQuery"/> к типам <paramref name="implementationTypes"/>.
        /// Установленные привязки используются при создании экземпляров объектов, цикл жизни которых не отслеживается.
        /// Если ранее уже была заданы привязки к <typeparamref name="TQuery"/>, то новый набор привязок заменяет старые.
        /// </summary>
        /// <typeparam name="TQuery">Тип, запрашиваемый при создании экземпляра объекта.</typeparam>
        /// <param name="implementationTypes">Список типов, используемых для создания экземпляров объектов при запросе типа <typeparamref name="TQuery"/>.</param>
        /// <exception cref="ArgumentNullException">Возникает, если параметр <paramref name="implementationTypes"/> не содержит элементов.</exception>
        /// <exception cref="ArgumentException">Возникает, если один из типов <paramref name="implementationTypes"/> не наследует <typeparamref name="TQuery"/>.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если обработчик привязок <see cref="IBindingConstraintHandler"/> (см. <see cref="RegisterBindingConstraintHandler(IBindingConstraintHandler)"/>) возвращает ошибку.</exception>
        public void SetTransient<TQuery>(params Type[] implementationTypes)
            where TQuery : IComponentTransient<TAppCore>
        {
            if (implementationTypes.Length == 0) throw new ArgumentNullException(nameof(implementationTypes));

            foreach (var type in implementationTypes)
            {
                //if (!typeof(ICoreComponentTransient<TAppCore>).IsAssignableFrom(type)) throw new ArgumentException($"Тип '{type.FullName}' не наследует '{typeof(ICoreComponentTransient<TAppCore>).FullName}'", nameof(implementationTypes));
                if (!typeof(TQuery).IsAssignableFrom(type)) throw new ArgumentException($"Тип '{type.FullName}' не наследует '{typeof(TQuery).FullName}'", nameof(implementationTypes));
                CheckBinding(typeof(TQuery), type);
            }

            var bindedTypes = implementationTypes.Select(x => new BindedType() { Type = x, Activator = CreateActivator(x) });
            _typesCollection[typeof(TQuery)] = new BindingDescription(bindedTypes);
        }

        /// <summary>
        /// Добавляет новую привязку типа <typeparamref name="TQuery"/> к <typeparamref name="TImplementation"/>.
        /// Установленная привязка используется при создании экземпляра объекта, цикл жизни которого не отслеживается.
        /// Если ранее уже были заданы привязки <typeparamref name="TQuery"/> к другим типам, то новая привязка добавляется к существующим.
        /// </summary>
        /// <typeparam name="TQuery">Тип, запрашиваемый при создании экземпляра объекта.</typeparam>
        /// <typeparam name="TImplementation">Тип, используемый при создании экземпляра объекта, приводимого к <typeparamref name="TQuery"/>.</typeparam>
        /// <exception cref="ArgumentException">Возникает, если <typeparamref name="TImplementation"/> обозначает абстрактный класс.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если обработчик привязок <see cref="IBindingConstraintHandler"/> (см. <see cref="RegisterBindingConstraintHandler(IBindingConstraintHandler)"/>) возвращает ошибку.</exception>
        public void AddTransient<TQuery, TImplementation>()
            where TQuery : IComponentTransient<TAppCore>
            where TImplementation : TQuery, new()
        {
            if (typeof(TImplementation).IsAbstract) throw new ArgumentException($"Параметр-тип {nameof(TImplementation)} не должен быть абстрактным типом.", nameof(TImplementation));

            CheckBinding(typeof(TQuery), typeof(TImplementation));

            _typesCollection.AddOrUpdate(
                typeof(TQuery),
                value => new BindingDescription(typeof(TImplementation), CreateActivator(typeof(TImplementation))) { Instances = null },
                (value, oldTypeDesctiption) =>
                {
                    var bindedTypes = oldTypeDesctiption.BindedTypes;
                    bindedTypes.Add(new BindedType() { Type = typeof(TImplementation), Activator = CreateActivator(typeof(TImplementation)) });
                    return new BindingDescription(bindedTypes) { Instances = null };
                }
            );
        }
        #endregion

        private Func<object> CreateActivator(Type type)
        {
            return new Func<object>(() => Activator.CreateInstance(type));
        }

        private void CheckBinding(Type queryType, Type implementedType)
        {
            if (!_constraintHandlers.IsNullOrEmpty())
            {
                foreach (var handler in _constraintHandlers)
                {
                    var e = new BindingConstraintEventArgs(queryType, implementedType);
                    handler.CheckBinding(this, e);
                    if (e._failed) throw new InvalidOperationException(e._message);
                }
            }
        }
        #endregion
    }
}
