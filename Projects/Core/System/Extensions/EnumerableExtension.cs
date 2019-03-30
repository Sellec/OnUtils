using System;
using System.Collections.Generic;

/// <summary>
/// </summary>
public static class EnumerableExtension
{
    /// <summary>
    /// Выполняет указанное действие <paramref name="action"/> с каждым элементом перечисления <paramref name="source"/>.
    /// </summary>
    /// <returns>Возвращает количество элементов, обработанных в перечислении.</returns>
    [System.Diagnostics.DebuggerStepThrough()]
    public static int ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        int counter = 0;
        if (source != null)
            foreach (var item in source)
            {
                try { action(item); }
                finally { counter++; }
            }
        return counter;
    }
}


