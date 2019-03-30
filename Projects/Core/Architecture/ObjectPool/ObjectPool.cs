using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnUtils.Architecture.ObjectPool
{
    /// <summary>
    /// Представляет пул объектов, реализующих интерфейс <typeparamref name="TPoolObject"/>.
    /// <para>При первичной инициализации (первое обращение к <see cref="ObjectList"/> с isLazyInitialization=true или вызов конструктора с isLazyInitialization=false) пул перебирает все сборки, 
    /// загруженные в текущий домен приложения, и создает экземпляры всех найденных типов, реализующих интерфейс <typeparamref name="TPoolObject"/>, и подписывается на событие <see cref="AppDomain.AssemblyLoad"/>, 
    /// повторяя процесс поиска подходящих типов для каждой новой сборки, загруженной в домен приложения.
    /// </para>
    /// <para>При создании экземпляра пул обращается к методу <see cref="OnCreatePoolObject{TPoolObjectConcrete}(ref bool)"/>, запрашивая экземпляр типа TPoolObjectConcrete, реализующего интерфейс <typeparamref name="TPoolObject"/>. Для подробностей см. описание метода.</para>
    /// </summary>
    public abstract class ObjectPool<TPoolObject> : IDisposable where TPoolObject : class, IPoolObject
    {
        private bool _disposed = false;
        private object SyncRoot = new object();
        private Lazy<List<TPoolObject>> _objects = null;

        /// <summary>
        /// Инициализация пула объектов с указанием режима инициализации списка объектов (ленивая или в конструкторе).
        /// </summary>
        /// <param name="isLazyInitialization">Указывает, как именно следует инициализировать список объектов - во время первого обращения к <see cref="ObjectList"/> (true) или непосредственно в конструкторе (false).</param>
        public ObjectPool(bool isLazyInitialization = true)
        {
            AppDomain.CurrentDomain.AssemblyLoad += UpdateObjectsListOnAssemblyLoad;

            _objects = new Lazy<List<TPoolObject>>(() =>
            {
                var objectsList = new List<TPoolObject>();
                if (typeof(TPoolObject) == typeof(IPoolObject)) throw new ArgumentException($"Параметр-тип {nameof(TPoolObject)} не должен совпадать с базовым типом {typeof(IPoolObject).FullName}, должен быть наследником.");
                if (!typeof(TPoolObject).IsInterface) throw new ArgumentException($"Параметр-тип {nameof(TPoolObject)} должен быть интерфейсом.");

                var types = GetObjectsTypesList();
                foreach (var type in types)
                {
                    try
                    {
                        var handled = false;
                        var onCreateFactoryMethod = typeof(ObjectPool<>).GetMethod(nameof(OnCreatePoolObject), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new Type[] { typeof(bool) }, null);
                        TPoolObject obj = null;
                        if (onCreateFactoryMethod != null)
                        {
                            onCreateFactoryMethod = onCreateFactoryMethod.MakeGenericMethod(type);
                            obj = onCreateFactoryMethod.Invoke(this, new object[] { handled }) as TPoolObject;
                        }

                        if (!handled)
                        {
                            var openConstructor = type.GetConstructor(new Type[] { });
                            if (openConstructor != null)
                            {
                                obj = Activator.CreateInstance(type) as TPoolObject;
                            }
                        }

                        if (obj != null)
                        {
                            if (obj is IPoolObjectInit) (obj as IPoolObjectInit).Init();
                            objectsList.Add(obj);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("{0}: provider '{1}' init error: {2}", this.GetType().FullName, type.FullName, ex.Message);
                    }
                }

                OnUpdateObjectsList(objectsList);
                return objectsList;
            }, true);

            OnInit();

            if (!isLazyInitialization)
            {
                var f = ObjectList.AsEnumerable().ToList();
            }
        }

        /// <summary>
        /// Этот метод вызывается при загрузке новой сборки в домен приложения и добавляет новые объекты на основании подходящих типов из загруженной сборки в список, но только в том случае, когда список уже загружен, чтобы не делать двойную работу.
        /// </summary>
        private void UpdateObjectsListOnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            try
            {
                if (!_objects.IsValueCreated || _disposed) return;

                lock (SyncRoot)
                {
                    var types = GetObjectTypesFromAssembly(args.LoadedAssembly);
                    if (types != null && types.Count() > 0)
                    {
                        var newObjects = types.Select(type =>
                        {
                            try
                            {
                                var handled = false;
                                var onCreateFactoryMethod = this.GetType().GetMethod(nameof(OnCreatePoolObject), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new Type[] { typeof(bool) }, null);
                                TPoolObject obj = null;
                                if (onCreateFactoryMethod != null)
                                {
                                    onCreateFactoryMethod = onCreateFactoryMethod.MakeGenericMethod(type);
                                    obj = onCreateFactoryMethod.Invoke(this, new object[] { handled }) as TPoolObject;
                                }

                                if (!handled)
                                {
                                    var openConstructor = type.GetConstructor(new Type[] { });
                                    if (openConstructor != null)
                                    {
                                        obj = Activator.CreateInstance(type) as TPoolObject;
                                    }
                                }

                                if (obj != null)
                                {
                                    if (obj is IPoolObjectInit) (obj as IPoolObjectInit).Init();
                                    return obj;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("{0}: provider '{1}' init error: {2}", this.GetType().FullName, type.FullName, ex.Message);
                            }

                            return null;
                        }).ToList();

                        if (newObjects.Count > 0)
                        {
                            _objects.Value.AddRange(newObjects);
                            OnUpdateObjectsList(newObjects);
                        }
                    }
                }
            }
            catch { }
        }

        private IEnumerable<Type> GetObjectsTypesList()
        {
            var typesList = LibraryEnumeratorFactory.Enumerate(GetObjectTypesFromAssembly, nameForLogging: this.GetType().FullName + "." + nameof(GetObjectsTypesList)).SelectMany(x => x).ToList();
            return typesList;
        }

        private IEnumerable<Type> GetObjectTypesFromAssembly(System.Reflection.Assembly assembly)
        {
            var types = Global.CheckIfIgnoredAssembly(assembly) ? null : assembly.GetTypes().Where(x => typeof(TPoolObject).IsAssignableFrom(x) && !x.IsAbstract && x.IsClass).ToList();
            return types;
        }

        /// <summary>
        /// Вызывается при инициализации пула объектов.
        /// </summary>
        protected virtual void OnInit()
        {
        }

        /// <summary>
        /// Вызывается после обновления списка объектов. 
        /// <para>Вызывается в нескольких случаях:</para>
        /// <para>1) Первый раз вызывается после первичной загрузки списка объектов после инициализации свойства <see cref="ObjectList"/>.</para>
        /// <para>2) Вызывается после загрузки новых сборок в домен приложения, если в сборке были обнаружены новые подходящие типы объектов.</para>
        /// </summary>
        protected virtual void OnUpdateObjectsList(IEnumerable<TPoolObject> objectsList)
        {
        }

        /// <summary>
        /// Вызывается, когда необходимо создать экземпляр объекта указанного типа.
        /// </summary>
        /// <typeparam name="TFactoryConcrete">Тип объекта, который необходимо создать.</typeparam>
        /// <param name="handled">Должно быть установлено в true, если указанный тип <typeparamref name="TFactoryConcrete"/> был обработан в методе. В противном случае будет предпринята попытка создать новый экземпляр типа <typeparamref name="TFactoryConcrete"/>, если для него имеется открытый беспараметрический конструктор. Если такого конструктора нет, то данный тип объекта будет пропущен.</param>
        /// <returns>Возвращает новый объект или null.</returns>
        protected virtual TFactoryConcrete OnCreatePoolObject<TFactoryConcrete>(ref bool handled) where TFactoryConcrete : class, TPoolObject
        {
            return null;
        }

        /// <summary>
        /// Вызывается при уничтожении пула объектов.
        /// </summary>
        protected virtual void OnDispose()
        {

        }

        void IDisposable.Dispose()
        {
            if (_disposed) return;

            try
            {
                OnDispose();
            }
            finally
            {
                _disposed = true;
                _objects = null;
            }

        }
        ///// <summary>
        ///// См. <see cref="IDisposable.Dispose()"/>.
        ///// </summary>
        //public virtual void Dispose()
        //{
        //}

        /// <summary>
        /// Возвращает список объектов, инициализированных пулом.
        /// Момент инициализации списка зависит от типа инициализации, переданного в конструктор <see cref="ObjectPool{TPoolObject}.ObjectPool(bool)"/>.
        /// </summary>
        public IEnumerable<TPoolObject> ObjectList
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException(this.GetType().Name, "Пул объектов уничтожен.");

                lock (SyncRoot)
                {
                    if (typeof(IPoolObjectOrdered).IsAssignableFrom(typeof(IPoolObjectOrdered)))
                        return _objects.Value.OrderBy(x => (x as IPoolObjectOrdered).OrderInPool).AsEnumerable();
                    else 
                        return _objects.Value.AsEnumerable();
                }
                    
            }
        }

    }

}
