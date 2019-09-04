using System;
using System.Collections.Concurrent;

namespace OnUtils.Application.Types
{
    /// <summary>
    /// Представляет потоконезависимую и потокобезопасную коллекцию флагов блокировки.
    /// </summary>
    public class ConcurrentFlagLocker<TFlag>
    {
        private ConcurrentDictionary<TFlag, int> _executingFlags = new ConcurrentDictionary<TFlag, int>();

        /// <summary>
        /// Возвращает true, если флаг был успешно заблокирован. Возвращает false, если блокировка флага невозможна (уже заблокирован другим процессом).
        /// </summary>
        public bool TryLock(TFlag flag)
        {
            return _executingFlags.AddOrUpdate(flag, 1, (k, o) => Math.Min(int.MaxValue, o + 1)) > 1 ? false : true;
        }

        /// <summary>
        /// Освобождает флаг от блокировки.
        /// </summary>
        /// <param name="flag"></param>
        public void ReleaseLock(TFlag flag)
        {
            _executingFlags[flag] = 0;
        }

    }
}
