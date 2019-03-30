using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnUtils.Utils
{
    /// <summary>
    /// </summary>
    public static class StringsHelper
    {
        /// <summary>
        /// Генерирует строку указанной длины <paramref name="length"/> из символов, содержащихся в <paramref name="chars"/>.
        /// </summary>
        /// <returns>Строка длиной <paramref name="length"/> из случайным образом выбранных символов, содержащихся в <paramref name="chars"/>.</returns>
        public static string GenerateRandomString(string chars, int length, int? seed = null)
        {
            if (string.IsNullOrEmpty(chars)) throw new ArgumentNullException(nameof(chars));
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length), "Длина должна быть больше нуля.");

            var random = seed.HasValue ? new Random(seed.Value) : new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Генерирует уникальный идентификатор (<see cref="Guid"/>) на основе строки <paramref name="source"/>. Если строка пустая (null или не содержит символов), то возвращаетя нулевой идентификатор <see cref="Guid.Empty"/>.
        /// </summary>
        public static Guid GenerateGuid(string source)
        {
            if (string.IsNullOrEmpty(source)) return Guid.Empty;

            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(source));
                return new Guid(hash);
            }
        }
    }
}
