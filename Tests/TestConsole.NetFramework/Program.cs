using System;
using System.Linq;

namespace TestConsole
{
    using OnUtils.Data;
    using OnUtils.Data.EntityFramework;
    using OnUtils.Data.UnitOfWork;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class res : IConnectionStringResolver
    {
        string IConnectionStringResolver.ResolveConnectionStringForDataContext(Type[] entityTypes)
        {
            return "Data Source=localhost;Initial Catalog=Test;Integrated Security=True;";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            DataAccessManager.SetConnectionStringResolver(new res());

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
