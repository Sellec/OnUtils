using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace OnUtils
{
    /// <summary>
    /// Предоставляет перечислитель для перебора всех связанных с приложением сборок для выполнения каких-либо действий в callback-методе.
    /// Перебирает ВСЕ возможные сборки: 
    /// 1) Загруженные в домене приложения;
    /// 2) Все связанные сборки;
    /// 3) Сборки, прикрепленные через Costura.Fody;
    /// 4) Сборки в папке приложения (см. <see cref="LibraryDirectory"/>).
    /// 
    /// Процесс перебора и загрузки выглядит следующим образом:
    /// 1) Получение всех загруженных сборок из домена приложения (<see cref="AppDomain.GetAssemblies"/>) и передача списка сборок в шаг 2;
    /// 2) Обработка списка переданных сборок:
    /// 2.1) Проверяется, следует ли игнорировать эту сборку в перечислителе (определенный список сборок некоторых расширений, также игнорируется VisualStudio DesignTime, вызов <see cref="GlobalAssemblyFilter"/>). Если сборка НЕ игнорируется, то переход в шаг 2.2, в противном случае к следующей сборке в списке;
    /// 2.2) Для сборки выполняется загрузка в домен приложения (<see cref="Assembly.Load(string)"/>) всех сборок, привязанных через Costura.Fody (зависимости-references прикрепляются как ресурсы). Список сборок, загруженных в этом шаге, рекурсивно передается в шаг 2 для загрузки дальше по иерархической цепочке;
    /// 2.3) Для сборки собирается список references (<see cref="Assembly.GetReferencedAssemblies"/>) и рекурсивно передается в шаг 2 для загрузки дальше по иерархической цепочке;
    /// 2.4) Проверяется, следует ли вызывать для сборки callback-метод. Проверка осуществляется на основе флага <see cref="LibraryEnumeratorFactory.EnumerateAttrs"/>, переданного при вызове перечислителя и на основе вызова <see cref="GlobalAssemblyFilter"/>. Если проверка пройдена, то сборка добавляется к списку для передачи в callback-метод;
    /// 3) После прохода по иерархической цепочке сборок перебирается список тех сборок, которые не были отфильтрованы в шаге 2.4 и для каждой вызывается callback-метод.
    /// 
    /// Процесс перебора и загрузки частично асинхронный, т.е. по возможности создается максимальное количество потоков для загрузки сборок. В некоторых случаях асинхронность игнорируется либо используется контекст синхронизации, т.к. могут возникать Deadlock при попытке загрузить зависимости сборки, которая находится в стеке вызовов.
    /// Шаг 3 полностью синхронный, т.е. callback-метод вызывается строго последовательно для каждой найденной сборки.
    /// </summary>
    public static class LibraryEnumeratorFactory
    {
        /// <summary>
        /// Перечисление действий, выполненных над сборкой перед вызовом Enumerate.callbackAfterLoad. 
        /// </summary>
        public enum ActionLoad
        {
            /// <summary>
            /// Сборка уже была ранее загружена в пул приложения, было выполнено только перечисление.
            /// </summary>
            Enumerated
        }

        /// <summary>
        /// Опции для записи в лог.
        /// </summary>
        [Flags]
        public enum eLoggingOptions
        {
            /// <summary>
            /// Без логирования.
            /// </summary>
            None, 

            /// <summary>
            /// Записывать в лог время, за которое были перебраны все сборки и отработан callback для первого вызова перечислителя.
            /// </summary>
            EnumerationSummaryFirstRun = 2,

            /// <summary>
            /// Записывать в лог время, за которое были перебраны все сборки и отработан callback.
            /// </summary>
            EnumerationSummary = 4,

            /// <summary>
            /// Записывать в лог сборки, которые были загружены методами <see cref="Assembly.Load(AssemblyName)"/> и пр.
            /// </summary>
            LoadAssembly = 16
        }

        /// <summary>
        /// Указывает, какие именно библиотеки следует включить в перечисление.
        /// </summary>
        [Flags]
        public enum EnumerateAttrs
        {
            /// <summary>
            /// Включает все библиотеки в перечисление по-умолчанию.
            /// </summary>
            Default,

            /// <summary>
            /// Исключает системные библиотеки из перечисления.
            /// </summary>
            ExcludeSystem,

            /// <summary>
            /// Исключает библиотеки с префиксом "Microsoft." из перечисления.
            /// </summary>
            ExcludeMicrosoft,

            /// <summary>
            /// Исключает известные сторонние библиотеки
            /// </summary>
            ExcludeKnownExternal
        }

        private static bool _executableDirectoryIsCustom = false;
        private static string _executableDirectoryCustom = string.Empty;
        private static volatile bool _isFirstFullEnumeration = false;

        static LibraryEnumeratorFactory()
        {
            InitLibraryEnumerator();
        }

        #region Методы
        private static void InitLibraryEnumerator()
        {
            LoggingOptions = eLoggingOptions.EnumerationSummaryFirstRun;

            var dir = Environment.CurrentDirectory;
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null ||
                dir.ToLower().Contains("iis") ||
                dir.ToLower().Contains("inetpub") ||
                dir.ToLower().Contains("wwwroot") ||
                dir.ToLower().Contains("inetsrv")
                )
            {
                var p = Path.GetDirectoryName((new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath);
                var p2 = Path.GetFileName(p).ToLower() == "bin" ? Path.GetDirectoryName(p) : p;

                _executableDirectoryCustom = p2;
                _executableDirectoryIsCustom = true;
            }
            else if (entryAssembly != null)
            {
                var p = Path.GetDirectoryName((new Uri(entryAssembly.CodeBase)).LocalPath);
                var p2 = Path.GetFileName(p).ToLower() == "bin" ? Path.GetDirectoryName(p) : p;

                _executableDirectoryCustom = p2;
                _executableDirectoryIsCustom = true;
            }

            GlobalAssemblyFilter = null;
        }

        /// <summary>
        /// Осуществляет перечисление сборок в папке <see cref="LibraryDirectory"/>. 
        /// </summary>
        /// <param name="callbackAfterLoad">Если задано, то вызывается для сборки, если callbackBeforeLoad не задано или возвратило true.</param>
        /// <param name="callbackBeforeLoad">Если задано, то вызывается для каждой найденной сборки. Если возвращает false, то обработка этоЙ сборки прекращается.</param>
        /// <param name="enumerateAttrs">Указывает, какие библиотеки следует включить в перечисление.</param>
        /// <param name="nameForLogging"></param>
        /// <param name="tasksAllowed"></param>
        public static void Enumerate(Action<Assembly> callbackAfterLoad, Func<string, bool> callbackBeforeLoad = null, EnumerateAttrs enumerateAttrs = EnumerateAttrs.ExcludeMicrosoft | EnumerateAttrs.ExcludeSystem, string nameForLogging = null, bool tasksAllowed = false)
        {
            var measure = new MeasureTime();
            try
            {
                var dddddd = new Reflection.LibraryEnumerator(callbackAfterLoad, callbackBeforeLoad, enumerateAttrs, GlobalAssemblyFilter, LoggingOptions, nameForLogging, tasksAllowed);
                dddddd.Enumerate();
            }
            catch (Exception)
            {
            }
            finally
            {
                if (!_isFirstFullEnumeration)
                {
                    _isFirstFullEnumeration = true;
                    if (LoggingOptions.HasFlag(eLoggingOptions.EnumerationSummaryFirstRun)) Debug.WriteLine("LibraryEnumeratorFactory.Enumerate: First enumeration ends with {0}ms", measure.Calculate().TotalMilliseconds);
                }
            }
        }

        /// <summary>
        /// Осуществляет перечисление сборок в папке <see cref="LibraryDirectory"/>. 
        /// </summary>
        /// <param name="callbackAfterLoad">Если задано, то вызывается для сборки, если callbackBeforeLoad не задано или возвратило true.</param>
        /// <param name="callbackBeforeLoad">Если задано, то вызывается для каждой найденной сборки. Если возвращает false, то обработка этоЙ сборки прекращается.</param>
        /// <param name="enumerateAttrs">Указывает, какие библиотеки следует включить в перечисление.</param>
        /// <param name="nameForLogging"></param>
        /// <param name="tasksAllowed"></param>
        public static IEnumerable<TResult> Enumerate<TResult>(Func<Assembly, TResult> callbackAfterLoad, Func<string, bool> callbackBeforeLoad = null, EnumerateAttrs enumerateAttrs = EnumerateAttrs.ExcludeMicrosoft | EnumerateAttrs.ExcludeSystem, string nameForLogging = null, bool tasksAllowed = false)
        {
            var results = new HashSet<TResult>();

            var measure = new MeasureTime();
            try
            {

                Action<Assembly> action = (a) =>
                {
                    var result = callbackAfterLoad(a);
                    if (result != null) results.Add(result);
                };

                var dddddd = new Reflection.LibraryEnumerator(action, callbackBeforeLoad, enumerateAttrs, GlobalAssemblyFilter, LoggingOptions, nameForLogging, tasksAllowed);
                dddddd.Enumerate();
            }
            catch (Exception)
            {
            }
            finally
            {
                if (!_isFirstFullEnumeration)
                {
                    _isFirstFullEnumeration = true;
                    if (LoggingOptions.HasFlag(eLoggingOptions.EnumerationSummaryFirstRun)) Debug.WriteLine("LibraryEnumeratorFactory.Enumerate: First enumeration ends with {0}ms", measure.Calculate().TotalMilliseconds);
                }
            }

            return results;
        }

        /// <summary>
        /// Возвращает сборку с указанным именем. Если сборка не загружена, то она загружается.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Assembly GetByFileName(string fileName)
        {
            Assembly ass = null;
            Enumerate((assembly) => ass = assembly, (filename) => Path.GetFileName(filename).ToLower().Equals(fileName.ToLower()), nameForLogging: nameof(LibraryEnumeratorFactory) + ".GetByFileName");
            return ass;
        }

#if NETSTANDARD2_0
#else
        private static bool hasWriteAccessToFolder(string folderPath)
        {
            try
            {
                // Attempt to get a list of security permissions from the folder. 
                // This will raise an exception if the path is read only or do not have access to view the permissions. 
                var ds = Directory.GetAccessControl(folderPath);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
#endif
        #endregion

        #region Свойства
        /// <summary>
        /// Указывает папку, содержащую исполняемые файлы приложения.
        /// 1)  Если свойство было явно задано, то возвращает явно заданную папку.
        /// 2)  Если свойство было явно задано как null, то возвращает <see cref="Environment.CurrentDirectory"/>.
        /// 3)  По-умолчанию при инициализации значение формируется следующим образом:
        /// 3.1)Если это приложение ASP.NET или приложение без точки входа (тоже признак ASP.NET), то используется папка, где расположена данная сборка. 
        ///     Если сборка расположена в папке с именем "bin", то используется уровень выше (т.е. содержащий "bin").
        /// 3.2)В остальных случаях используется рабочая папка (<see cref="Environment.CurrentDirectory"/>).
        /// </summary>
        public static string LibraryDirectory
        {
            get { return _executableDirectoryIsCustom ? _executableDirectoryCustom : Environment.CurrentDirectory; }
            set
            {
                if (value == null) _executableDirectoryIsCustom = false;
                else
                {
                    _executableDirectoryIsCustom = true;
                    _executableDirectoryCustom = value;
                }
            }
        }

        /// <summary>
        /// Позволяет получить или установить глобальный фильтр сборок. Этот фильтр определяет, какие сборки исключаются из перечислений и автоматической загрузки при переборе дерева иерархии.
        /// Принимаемый аргумент представляет собой имя сборки. Если возвращает false, то сборка пропускается.
        /// 
        /// Следует учесть, что перечислитель используется в инициализаторе <see cref="Startup.StartupFactory"/>. 
        /// Если инициализатор запускается автоматически (см. <see cref="Startup.StartupBehaviourAttribute"/>), <see cref="GlobalAssemblyFilter"/> не может быть установлен ДО вызова инициализатора. Таким образом, при автоматическом запуске инициализатора невозможно воспользоваться <see cref="GlobalAssemblyFilter"/>.
        /// Если необходимо инициализироваться с использованием <see cref="GlobalAssemblyFilter"/>, следует установить атрибут <see cref="Startup.StartupBehaviourAttribute"/> с флагом <see cref="Startup.StartupBehaviourAttribute.IsNeedStartupFactoryAuto"/> и запустить инициализатор вручную после установки фильтра.
        /// </summary>
        public static Func<string, bool> GlobalAssemblyFilter
        {
            get;
            set;
        }

        /// <summary>
        /// Устанавливает глобальные настройки логирования во время перебора сборок.
        /// </summary>
        public static eLoggingOptions LoggingOptions
        {
            get;
            set;
        }
        #endregion
    }
}
