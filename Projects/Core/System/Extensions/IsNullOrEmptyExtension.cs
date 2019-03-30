using System.Collections;
using System.Collections.Generic;

/// <summary>
/// </summary>
public static class IsNullOrEmptyExtension
{
    /// <summary>
    /// Указывает, является ли указанное перечисление пустым или равным null.
    /// </summary>
    public static bool IsNullOrEmpty(this IEnumerable source)
    {
        if (source != null)
        {
            foreach (object obj in source)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Указывает, является ли указанное перечисление пустым или равным null.
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
    {
        if (source != null)
        {
            foreach (T obj in source)
            {
                return false;
            }
        }
        return true;
    }
}