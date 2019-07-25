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
            return "Data Source=localhost;Initial Catalog=Dombonus_OnWeb;Integrated Security=True;";
        }
    }

    public class app : OnUtils.Application.ApplicationCore<app>
    {
        public app(string physicalApplicationPath) : base(physicalApplicationPath)
        {
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            DataAccessManager.SetConnectionStringResolver(new res());

            var app = new app(Environment.CurrentDirectory);
            app.Start();

            app.Stop();
            var d = new ccc();
            var ddd = d.Repo1.Where(x => x.IdModule >= 1).Take(2).ToList();
            ddd.First().IdUserChange = 123133;
            d.SaveChanges();

            Console.WriteLine("Hello World!");

            Console.ReadKey();
        }
    }
}
