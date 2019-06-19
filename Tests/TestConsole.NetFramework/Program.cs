using System;
using System.Linq;

namespace TestConsole
{
    using OnUtils.Data;
    using OnUtils.Data.EntityFramework;
    using OnUtils.Data.UnitOfWork;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using TestConsole.QueryExtensions;

    [Table("Realty")]
    public class Realty : QueryExtensions.ItemBase, QueryExtensions.IItemBaseRealtyType, IItemBaseUrlTranslation
    {
        [Key]
        [Column("id")]
        public override int ID
        {
            get;
            set;
        }

        public string name { get; set; }

        [NotMapped]
        public int? IdRealtyType { get; set; }

        [NotMapped]
        public Uri Url{ get; set; }
    }


    public class ccc : UnitOfWork<Realty, QueryExtensions.UrlTranslation, QueryExtensions.RealtyTypeItem>
    {
        protected override void OnModelCreating(IModelAccessor modelAccessor)
        {
            //modelAccessor.UseEntityFramework(modelBuilder =>
            //{
            //    modelBuilder.Entity<ModuleConfig>().HasKey(x => new { x.IdModule });
            //});
        }
    }

    public class res : IConnectionStringResolver
    {
        string IConnectionStringResolver.ResolveConnectionStringForDataContext(Type[] entityTypes)
        {
            return "Data Source=localhost;Initial Catalog=Dombonus_OnWeb;Integrated Security=True;";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            DataAccessManager.SetConnectionStringResolver(new res());

            Debug.DebugSQL = true;

            var dbContext = new ccc();

            var results = QueryExtensions.QueryBuilder.CreateQuery<Realty>(dbContext.Repo1.Where(x => x.ID == 44208), dbContext);

            var d = 0;

            //var ddd = from item in d.Repo1
            //          where item.ID == 44208
            //          select new QueryResult<Realty>()
            //          {
            //              Item = item
            //          };

            //var dddResult = ddd.ToList();

            //var ddd = d.Repo1.Where(x => x.ID == 44208).ToList();
            //ddd.First().IdUserChange = 123133;
            //d.SaveChanges();

            Console.WriteLine("Hello World!");

            Console.ReadKey();
        }
    }
}
