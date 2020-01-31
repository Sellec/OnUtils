using System;

namespace OnUtils.Architecture.AppCore.DI
{
    /// <summary>
    /// Позволяет управлять привязкой классов для создании экземпляров singleton-объектов, существующего весь цикл жизни ядра.
    /// </summary>
    public interface ISingletonBindingsHandler<in TAppCore>
    {
        /// <summary>
        /// Устанавливает привязку типа <typeparamref name="TQuery"/>. Тип <typeparamref name="TQuery"/> используется в качестве queryType и implementationType, см. описание <see cref="SetSingleton{TQuery, TImplementation}()"/>.
        /// Установленная привязка используется при создании экземпляра singleton-объекта, существующего весь цикл жизни ядра.
        /// Если ранее уже была задана привязка к <typeparamref name="TQuery"/>, то новая привязка заменяет старую.
        /// </summary>
        /// <typeparam name="TQuery">Тип, запрашиваемый при создании экземпляра объекта и используемый при создании экземпляра объекта.</typeparam>
        /// <exception cref="ArgumentException">Возникает, если <typeparamref name="TQuery"/> обозначает абстрактный класс.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если обработчик привязок <see cref="IBindingConstraintHandler"/> (см. <see cref="IBindingsCollection{TAppCore}.RegisterBindingConstraintHandler(IBindingConstraintHandler)"/>) возвращает ошибку.</exception>
        void SetSingleton<TQuery>()
            where TQuery : class, IComponentSingleton<TAppCore>;

        /// <summary>
        /// Устанавливает привязку типа <typeparamref name="TQuery"/>. Тип <typeparamref name="TQuery"/> используется в качестве queryType и implementationType, см. описание <see cref="SetSingleton{TQuery, TImplementation}(Func{TImplementation})"/>.
        /// Установленная привязка используется при создании экземпляра singleton-объекта, существующего весь цикл жизни ядра.
        /// Если ранее уже была задана привязка к <typeparamref name="TQuery"/>, то новая привязка заменяет старую.
        /// </summary>
        /// <typeparam name="TQuery">Тип, запрашиваемый при создании экземпляра объекта и используемый при создании экземпляра объекта.</typeparam>
        /// <param name="factoryLambda">Фабрика, используемая при получении экземпляра объекта.</param>
        /// <exception cref="ArgumentException">Возникает, если <typeparamref name="TQuery"/> обозначает абстрактный класс.</exception>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="factoryLambda"/> равен null.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если обработчик привязок <see cref="IBindingConstraintHandler"/> (см. <see cref="IBindingsCollection{TAppCore}.RegisterBindingConstraintHandler(IBindingConstraintHandler)"/>) возвращает ошибку.</exception>
        void SetSingleton<TQuery>(Func<TQuery> factoryLambda)
            where TQuery : class, IComponentSingleton<TAppCore>;

        /// <summary>
        /// Устанавливает привязку типа <typeparamref name="TQuery"/> к <typeparamref name="TImplementation"/>.
        /// Установленная привязка используется при создании экземпляра singleton-объекта, существующего весь цикл жизни ядра.
        /// Если ранее уже была задана привязка к <typeparamref name="TQuery"/>, то новая привязка заменяет старую.
        /// </summary>
        /// <typeparam name="TQuery">Тип, запрашиваемый при создании экземпляра объекта.</typeparam>
        /// <typeparam name="TImplementation">Тип, используемый при создании экземпляра объекта, приводимого к <typeparamref name="TQuery"/>.</typeparam>
        /// <exception cref="ArgumentException">Возникает, если <typeparamref name="TImplementation"/> обозначает абстрактный класс.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если обработчик привязок <see cref="IBindingConstraintHandler"/> (см. <see cref="IBindingsCollection{TAppCore}.RegisterBindingConstraintHandler(IBindingConstraintHandler)"/>) возвращает ошибку.</exception>
        void SetSingleton<TQuery, TImplementation>()
            where TQuery : IComponentSingleton<TAppCore>
            where TImplementation : TQuery;

        /// <summary>
        /// Устанавливает привязку типа <typeparamref name="TQuery"/> к <typeparamref name="TImplementation"/>.
        /// Установленная привязка используется при создании экземпляра singleton-объекта, существующего весь цикл жизни ядра.
        /// Если ранее уже была задана привязка к <typeparamref name="TQuery"/>, то новая привязка заменяет старую.
        /// </summary>
        /// <typeparam name="TQuery">Тип, запрашиваемый при создании экземпляра объекта.</typeparam>
        /// <typeparam name="TImplementation">Тип, используемый при создании экземпляра объекта, приводимого к <typeparamref name="TQuery"/>.</typeparam>
        /// <param name="factoryLambda">Фабрика, используемая при получении экземпляра объекта.</param>
        /// <exception cref="ArgumentException">Возникает, если <typeparamref name="TImplementation"/> обозначает абстрактный класс.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если обработчик привязок <see cref="IBindingConstraintHandler"/> (см. <see cref="IBindingsCollection{TAppCore}.RegisterBindingConstraintHandler(IBindingConstraintHandler)"/>) возвращает ошибку.</exception>
        void SetSingleton<TQuery, TImplementation>(Func<TImplementation> factoryLambda)
            where TQuery : IComponentSingleton<TAppCore>
            where TImplementation : TQuery;

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
        /// <exception cref="InvalidOperationException">Возникает, если обработчик привязок <see cref="IBindingConstraintHandler"/> (см. <see cref="IBindingsCollection{TAppCore}.RegisterBindingConstraintHandler(IBindingConstraintHandler)"/>) возвращает ошибку.</exception>
        void SetSingleton(Type queryType, Type implementationType);
    }
}
