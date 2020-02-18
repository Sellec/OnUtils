using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;
using OnUtils.Data;
using OnUtils.Data.EntityFramework;
using OnUtils.Data.UnitOfWork;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TestConsole
{
    [Table("ModuleConfig")]
    public partial class ModuleConfig
    {
        /// <summary>
        /// Идентификатор модуля.
        /// </summary>
       //[Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdModule { get; set; }

        /// <summary>
        /// Уникальное значение, позволяющее идентифицировать query-тип модуля. Используется полное имя query-типа.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string UniqueKey { get; set; }

        /// <summary>
        /// Сериализованные в json параметры конфигурации модуля. См. <see cref="Configuration.ModuleConfiguration{TModule}"/>.
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// Дата последнего изменения записи в базе.
        /// </summary>
        public DateTime DateChange { get; set; }

        /// <summary>
        /// Идентификатор пользователя, менявшего параметры в последний раз.
        /// </summary>
        public int IdUserChange { get; set; }

    }

    public class ccc : UnitOfWork<ModuleConfig>
    {
        protected override void OnModelCreating(IModelAccessor modelAccessor)
        {
            modelAccessor.ConnectionString= "Data Source=localhost;Initial Catalog=Test;Integrated Security=True;";
            modelAccessor.UseEntityFramework(modelBuilder =>
            {
                modelBuilder.Entity<ModuleConfig>().HasKey(x => new { x.IdModule });
            });
        }
    }

    public class res : IConnectionStringResolver
    {
        string IConnectionStringResolver.ResolveConnectionStringForDataContext(Type[] entityTypes)
        {
            return "Data Source=localhost;Initial Catalog=Test;Integrated Security=True;";
        }
    }

    class apptest : AppCore<apptest>
    {
        //protected override IBindingsResolver<apptest> GetBindingsResolver()
        //{
        //    return new resolver();
        //}
    }

    class t : CoreComponentBase<apptest>, IComponentSingleton<apptest>
    {
    }

    class startup : IConfigureBindings<apptest>
    {
        void IConfigureBindings<apptest>.ConfigureBindings(IBindingsCollection<apptest> bindingsCollection)
        {
    //        bindingsCollection.SetSingleton<t>();
        }
    }

    class resolver : IBindingsResolver<apptest>
    {
        void IBindingsResolver<apptest>.OnSingletonBindingResolve<TRequestedType>(ISingletonBindingsHandler<apptest> bindingsHandler)
        {
            if (typeof(TRequestedType) == typeof(t)) bindingsHandler.SetSingleton<t>();
        }

        void IBindingsResolver<apptest>.OnTransientBindingResolve<TRequestedType>(ITransientBindingsHandler<apptest> bindingsHandler)
        {
   //         if (typeof(TRequestedType) == typeof(t)) bindingsHandler.SetTransient<t>();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            DataAccessManager.SetConnectionStringResolver(new res());

            //var d = new ccc();
            //var ddd = d.Repo1.Where(x => x.IdModule >= 1).Take(2).ToList();
            //ddd.First().IdUserChange = 123133;
            //d.SaveChanges();

            //var appcore = new apptest();
            //appcore.Start();
            //appcore.SetBindingsResolver(new resolver());
            //var instance = appcore.Get<t>();

            var appCoreLazyTest = new LazyBinding.App.ApplicationCore();
            appCoreLazyTest.Start();

            System.Reflection.Assembly.Load("TestConsole.NetFramework.LazyBinding");
            var queryTypes = appCoreLazyTest.GetQueryTypes();
            var bindedTypes1 = appCoreLazyTest.GetBindedTypes<LazyBinding.App.ITestComponent1>();
            var bindedTypes2 = appCoreLazyTest.GetBindedTypes<LazyBinding.App.ITestComponent2>();

            Console.WriteLine("Hello World!");

            Console.ReadKey();
        }
    }
}
