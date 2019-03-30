using System.Text;

namespace System
{
    /// <summary>
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Возвращает часть строки <paramref name="str"/>, начиная с указанной позиции <paramref name="start"/> длиной НЕ БОЛЕЕ <paramref name="limit"/>.
        /// Оличается от <see cref="string.Substring(int, int)"/> тем, что позволяет указать произвольный <paramref name="limit"/>. 
        /// Если длина строки меньше, чем <paramref name="start"/> + <paramref name="limit"/>, будет возвращена только имеющаяся часть. <see cref="string.Substring(int, int)"/> же в этом случае сгенерирует исключение.
        /// Если <paramref name="suffix"/> задан и возвращаемая строка была обрезана, то <paramref name="suffix"/> добавляется в конец новой строки.
        /// </summary>
        public static string Truncate(this string str, int start, int limit = 0, string suffix = null)
        {
            if (limit < 0) limit = 0;

            var _str = str;
            if (string.IsNullOrEmpty(_str)) _str = string.Empty;
            if (_str.Length <= start) return string.Empty;

            _str = _str.Substring(start);
            if (limit == 0) return _str;
            if (_str.Length > limit) return _str.Substring(0, limit) + suffix;
            return _str;
        }

        /// <summary>
        /// Возвращает новую строку, в которой все вхождения заданных знаков Юникода <paramref name="search"/> заменены другими заданными знаками Юникода <paramref name="replace"/>.
        /// Если <paramref name="replace"/> не задан или его длина меньше, чем длина <paramref name="search"/>, то все вхождения из <paramref name="search"/>, для которых не было найдено соответствий в <paramref name="replace"/>, будут заменены на пустой знак.
        /// </summary>
        public static string Replace(this string str, char[] search, char[] replace)
        {
            var strCopy = str;
            if (search != null)
                for (int i = 0; i < search.Length; i++)
                    strCopy = strCopy.Replace(search[i], replace != null && replace.Length > i ? replace[i] : '\0');

            return strCopy;
        }

        /// <summary>
        /// Возвращает новую строку, в которой все вхождения заданных строк <paramref name="search"/> заменены другими заданными строками <paramref name="replace"/>.
        /// Если <paramref name="replace"/> не задан или его длина меньше, чем длина <paramref name="search"/>, то все вхождения из <paramref name="search"/>, для которых не было найдено соответствий в <paramref name="replace"/>, будут заменены на пустую строку.
        /// </summary>
        public static string Replace(this string str, string[] search, string[] replace)
        {
            var strCopy = str;
            if (search != null)
                for (int i = 0; i < search.Length; i++)
                    strCopy = strCopy.Replace(search[i], replace != null && replace.Length > i ? replace[i] : string.Empty);

            return strCopy;
        }

        /// <summary>
        /// Возвращает исходную строку, преобразуя первый символ в нижний регистр.
        /// </summary>
        public static string ToLowerFirstCharacter(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;

            return Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        /// <summary>
        /// Генерирует уникальный идентификатор (<see cref="Guid"/> из текущей строки). Если строка пустая (null или не содержит символов), то возвращается <see cref="Guid.Empty"/>.
        /// </summary>
        public static Guid GenerateGuid(this string str)
        {
            return OnUtils.Utils.StringsHelper.GenerateGuid(str);
        }

        /// <summary>
        /// Создает MD5-хеш на основе текущей строки.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string MD5(this object input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            var md5Hasher = Security.Cryptography.MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input?.ToString()));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}