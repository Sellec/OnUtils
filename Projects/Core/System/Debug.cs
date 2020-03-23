using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace System
{
    /// <summary>
    /// Отладочный вывод и запись в лог.
    /// </summary>
    public static class Debug
    {
        private static Action<string, object[]> _debugWriter = null;
        private static ConcurrentDictionary<string, object> _logsSyncRoots = new ConcurrentDictionary<string, object>();
        private static bool _logsSourceDirectoryIsCustom = false;
        private static string _logsSourceDirectoryCustom = string.Empty;
        private static volatile bool _isDeveloper = Environment.MachineName.ToLower() == "sellsnote" || Environment.MachineName.ToLower() == "sellspc" || Environment.MachineName.ToLower() == "itegr13" || Environment.MachineName.ToLower() == "asnoteal";

        static Debug()
        {
            EnableLoggingOnDebugOutput = true;
            EnableAdditionalCommonLog = true;

            var dir = Environment.CurrentDirectory;
            if (Assembly.GetEntryAssembly() == null ||
                !hasWriteAccessToFolder(dir) ||
                dir.ToLower().Contains("iis") ||
                dir.ToLower().Contains("inetpub") ||
                dir.ToLower().Contains("wwwroot") ||
                dir.ToLower().Contains("inetsrv")
                )
            {
                var p = Path.GetDirectoryName((new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath);
                var p2 = Path.GetFileName(p).ToLower() == "bin" ? Path.GetDirectoryName(p) : p;

                if (hasWriteAccessToFolder(p2)) _logsSourceDirectoryCustom = p2;
                else _logsSourceDirectoryCustom = p;

                _logsSourceDirectoryIsCustom = true;
            }

            if (IsDeveloper)
            {
                var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (!string.IsNullOrEmpty(desktopFolder))
                {
                    _logsSourceDirectoryCustom = desktopFolder;
                    _logsSourceDirectoryIsCustom = true;
                }
            }
        }

        #region Вывод в консоль
        /// <summary>
        /// Запись данных в консоль вывода и в лог-файлы.
        /// </summary>
        public static void WriteLine(string str, params object[] values)
        {
            var strtr = values != null && values.Length > 0 ? string.Format(str, values) : str;

            try
            {
                if (_debugWriter != null) _debugWriter(str, values);
                else
                {
                    var ss = string.Format("ThreadID={0}: ", Thread.CurrentThread.ManagedThreadId) + strtr;
                    System.Diagnostics.Debug.WriteLine(ss.Trim());
                    if (EnableLoggingOnDebugOutput) Logs(ss.Trim());
                }
            }
            catch
            {
                var ss = string.Format("ThreadID={0}: ", Thread.CurrentThread.ManagedThreadId) + strtr;
                System.Diagnostics.Debug.WriteLine(ss.Trim());
                if (EnableLoggingOnDebugOutput) Logs(ss.Trim());
            }
        }

        /// <summary>
        /// Запись данных в консоль вывода и в лог-файлы.
        /// </summary>
        public static void WriteLine(object obj)
        {
            if (obj != null) WriteLine(obj.ToString());
        }

        /// <summary>
        /// Запись данных ТОЛЬКО в консоль вывода.
        /// </summary>
        public static void WriteLineNoLog(string str, params object[] values)
        {
            var strtr = values != null && values.Length > 0 ? string.Format(str, values) : str;

            try
            {
                if (_debugWriter != null) _debugWriter(str, values);
                else
                {
                    var ss = string.Format("ThreadID={0}: ", Thread.CurrentThread.ManagedThreadId) + strtr;
                    System.Diagnostics.Debug.WriteLine(ss.Trim());
                }
            }
            catch
            {
                var ss = string.Format("ThreadID={0}: ", Thread.CurrentThread.ManagedThreadId) + strtr;
                System.Diagnostics.Debug.WriteLine(ss.Trim());
            }
        }

        /// <summary>
        /// Запись данных ТОЛЬКО в консоль вывода.
        /// </summary>
        public static void WriteLineNoLog(object obj)
        {
            if (obj != null) WriteLineNoLog(obj.ToString());
        }
        #endregion

        #region Logs
        /// <summary>
        /// Записывает сообщение <paramref name="message"/>, отформатированное с использованием <see cref="string.Format(string, object[])"/>, в лог-файл.
        /// Более подробно см. <see cref="Logs(string)"/>.
        /// </summary>
        public static void Logs(string message, params object[] _params)
        {
            Logs(string.Format(message, _params));
        }

        /// <summary>
        /// Записывает сообщение <paramref name="message"/> в лог-файл.
        /// Имя лог-файла
        /// </summary>
        /// <param name="message"></param>
        public static void Logs(string message)
        {
            try
            {
                var trace = new StackTrace(1, true);
                for (int i = 1; i < trace.FrameCount; i++)
                {
                    var frame = trace.GetFrame(i);
                    var declaringType = frame.GetMethod().DeclaringType;
                    if (declaringType == typeof(Debug)) continue;

                    var filename = declaringType.FullName + ".log";

                    filename = Path.GetInvalidFileNameChars().Aggregate(filename, (current, c) => current.Replace(c.ToString(), string.Empty));
                    filename = Path.GetInvalidPathChars().Aggregate(filename, (current, c) => current.Replace(c.ToString(), string.Empty));

                    string funcname = frame.GetMethod().Name;
                    var tt = string.Format("{0:dd.MM.yyyy HH:mm:ss}. '{1}': {2}\r\n", DateTime.Now, funcname, message);

                    LogsWriteToFile(filename, tt);
                    if (EnableAdditionalCommonLog)
                    {
                        tt = string.Format("{0:dd.MM.yyyy HH:mm:ss}. '{1}.{2}': {3}\r\n", DateTime.Now, declaringType.FullName, funcname, message);
                        LogsWriteToFile("_CommonLog.log", tt);
                    }

                    break;
                }
            }
            catch { throw; }
        }

        private static void LogsWriteToFile(string filename, string message)
        {
            var dir = LogsDirectory;

            lock (_logsSyncRoots.GetOrAdd(filename, new object()))
            {
                Directory.CreateDirectory(dir);
                File.AppendAllText(Path.Combine(dir, filename), message.Trim() + "\r\n", System.Text.Encoding.UTF8);
            }
        }

        private static bool hasWriteAccessToFolder(string folderPath)
        {
            try
            {
                // Attempt to get a list of security permissions from the folder. 
                // This will raise an exception if the path is read only or do not have access to view the permissions. 
#if NETSTANDARD2_0
#else
                System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(folderPath);
#endif
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

#endregion

#region Property
        /// <summary>
        /// Это машина разработчика, значит надо менять папку логов на рабочий стол.
        /// </summary>
        public static bool IsDeveloper
        {
            get => _isDeveloper;
        }

        /// <summary>
        /// Надо ли записывать в лог при вызове WriteLine.
        /// </summary>
        public static bool EnableLoggingOnDebugOutput
        {
            get;
            set;
        }

        /// <summary>
        /// Надо ли писать в общий файл _CommonLog.log помимо отдельных лог-файлов
        /// </summary>
        public static bool EnableAdditionalCommonLog
        {
            get;
            set;
        }

        /// <summary>
        /// Путь к папке с логами.
        /// Базовая папка (содержащая папку "Logs") определяется следующим образом:
        /// 1)  Если это приложение ASP.NET или приложение без точки входа (тоже признак ASP.NET), то используется папка, где расположена данная сборка. 
        ///     Если сборка расположена в папке с именем "bin", то используется уровень выше (т.е. содержащий "bin").
        /// 2)  В остальных случаях используется рабочая папка (<see cref="Environment.CurrentDirectory"/>).
        /// </summary>
        public static string LogsDirectory
        {
            get => Path.Combine(_logsSourceDirectoryIsCustom ? _logsSourceDirectoryCustom : Environment.CurrentDirectory, "Logs");
        }

#endregion
    }
}
