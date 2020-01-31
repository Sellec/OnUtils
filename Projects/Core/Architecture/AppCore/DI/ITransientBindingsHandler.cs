using System;

namespace OnUtils.Architecture.AppCore.DI
{
    /// <summary>
    /// Позволяет управлять привязкой классов для создания множественных экземпляров объектов, цикл жизни которых не отслеживается.
    /// </summary>
    public interface ITransientBindingsHandler<in TAppCore>
    {
        /// <summary>
        /// Устанавливает привязку типа <typeparamref name="TQuery"/>. Тип <typeparamref name="TQuery"/> используется в качестве queryType и implementationType, см. описание <see cref="SetTransient{TQuery, TImplementation}"/>.
        /// Установленная привязка используется при создании экземпляров объектов, цикл жизни которых не отслеживается.
        /// Если ранее уже были заданы привязки к <typeparamref name="TQuery"/>, то новая привязка заменяет старые.
        /// </summary>
        /// <typeparam name="TQuery">Тип, запрашиваемый при создании экземпляра объекта и используемый при создании экземпляра объекта.</typeparam>
        /// <exception cref="ArgumentException">Возникает, если <typeparamref name="TQuery"/> обозначает абстрактный класс.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если обработчик привязок <see cref="IBindingConstraintHandler"/> (см. <see cref="IBindingsCollection{TAppCore}.RegisterBindingConstraintHandler(IBindingConstraintHandler)"/>) возвращает ошибку.</exception>
        void SetTransient<TQuery>()
            where TQuery : class, IComponentTransient<TAppCore>;

        /// <summary>
        /// Устанавливает привязку типа <typeparamref name="TQuery"/> к <typeparamref name="TImplementation"/>.
        /// Установленная привязка используется при создании экземпляров объектов, цикл жизни которых не отслеживается.
        /// Если ранее уже были заданы привязки к <typeparamref name="TQuery"/>, то новая привязка заменяет старые.
        /// </summary>
        /// <typeparam name="TQuery">Тип, запрашиваемый при создании экземпляра объекта.</typeparam>
        /// <typeparam name="TImplementation">Тип, используемый при создании экземпляра объекта, приводимого к <typeparamref name="TQuery"/>.</typeparam>
        /// <exception cref="ArgumentException">Возникает, если <typeparamref name="TImplementation"/> обозначает абстрактный класс.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если обработчик привязок <see cref="IBindingConstraintHandler"/> (см. <see cref="IBindingsCollection{TAppCore}.RegisterBindingConstraintHandler(IBindingConstraintHandler)"/>) возвращает ошибку.</exception>
        void SetTransient<TQuery, TImplementation>()
            where TQuery : IComponentTransient<TAppCore>
            where TImplementation : TQuery;

        /// <summary>
        /// Устанавливает привязки типа <typeparamref name="TQuery"/> к типам <paramref name="implementationTypes"/>.
        /// Установленные привязки используются при создании экземпляров объектов, цикл жизни которых не отслеживается.
        /// Если ранее уже была заданы привязки к <typeparamref name="TQuery"/>, то новый набор привязок заменяет старые.
        /// </summary>
        /// <typeparam name="TQuery">Тип, запрашиваемый при создании экземпляра объекта.</typeparam>
        /// <param name="implementationTypes">Список типов, используемых для создания экземпляров объектов при запросе типа <typeparamref name="TQuery"/>.</param>
        /// <exception cref="ArgumentNullException">Возникает, если параметр <paramref name="implementationTypes"/> не содержит элементов.</exception>
        /// <exception cref="ArgumentException">Возникает, если один из типов <paramref name="implementationTypes"/> не наследует <typeparamref name="TQuery"/>.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если обработчик привязок <see cref="IBindingConstraintHandler"/> (см. <see cref="IBindingsCollection{TAppCore}.RegisterBindingConstraintHandler(IBindingConstraintHandler)"/>) возвращает ошибку.</exception>
        void SetTransient<TQuery>(params Type[] implementationTypes)
            where TQuery : IComponentTransient<TAppCore>;

        /// <summary>
        /// Добавляет новую привязку типа <typeparamref name="TQuery"/> к <typeparamref name="TImplementation"/>.
        /// Установленная привязка используется при создании экземпляра объекта, цикл жизни которого не отслеживается.
        /// Если ранее уже были заданы привязки <typeparamref name="TQuery"/> к другим типам, то новая привязка добавляется к существующим.
        /// </summary>
        /// <typeparam name="TQuery">Тип, запрашиваемый при создании экземпляра объекта.</typeparam>
        /// <typeparam name="TImplementation">Тип, используемый при создании экземпляра объекта, приводимого к <typeparamref name="TQuery"/>.</typeparam>
        /// <exception cref="ArgumentException">Возникает, если <typeparamref name="TImplementation"/> обозначает абстрактный класс.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если обработчик привязок <see cref="IBindingConstraintHandler"/> (см. <see cref="IBindingsCollection{TAppCore}.RegisterBindingConstraintHandler(IBindingConstraintHandler)"/>) возвращает ошибку.</exception>
        void AddTransient<TQuery, TImplementation>()
            where TQuery : IComponentTransient<TAppCore>
            where TImplementation : TQuery, new();
    }
}
