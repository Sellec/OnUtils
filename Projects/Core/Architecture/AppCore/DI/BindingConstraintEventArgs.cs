using System;

namespace OnUtils.Architecture.AppCore.DI
{
    /// <summary>
    /// Описывает данные для события добавления привязки. Если обработчик события вызывает <see cref="SetFailed(string)"/>, то дальнейшая обработка привязки останавливается и генерируется исключение <see cref="InvalidOperationException"/>.
    /// </summary>
    public class BindingConstraintEventArgs : EventArgs
    {
        internal bool _failed = false;
        internal string _message = string.Empty;

        internal BindingConstraintEventArgs(Type queryType, Type implementedType)
        {
            QueryType = queryType;
            ImplementedType = implementedType;
        }

        /// <summary>
        /// Тип, запрашиваемый при попытке получить экземпляр объекта.
        /// </summary>
        public Type QueryType { get; set; }

        /// <summary>
        /// Тип, привязанный к <see cref="QueryType"/>. Объект этого типа возвращается при попытке запросить <see cref="QueryType"/>.
        /// </summary>
        public Type ImplementedType { get; set; }

        /// <summary>
        /// Устанавливает статус ошибки текущей привязки. 
        /// Обработка события другими обработчиками <see cref="IBindingConstraintHandler"/> останавливается и генерируется исключение <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="message">Описание ошибки привязки. Не должно быть пустым.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="message"/> пуст или равен null.</exception>
        public void SetFailed(string message)
        {
            if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));

            _failed = true;
            _message = message;
        }
    }
}
