using Microsoft.EntityFrameworkCore;
using OnUtils.Data;
using OnUtils.Data.EntityFramework;
using OnUtils.Data.UnitOfWork;
using System;
using System.Linq;

namespace TestConsole.NetCore.Data.EFCore
{
    public class ccc : UnitOfWorkBase
    {
        protected override void OnModelCreating(IModelAccessor modelAccessor)
        {
            modelAccessor.UseEntityFrameworkCore(
                optionsBuilder =>
                {
                    optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=TestEfCore;Integrated Security=True;");
                },
                modelBuilder =>
                {
                });
        }

        public IRepository<TestDecimal> TestDecimal { get; set; }
        public IRepository<TestToDouble> TestToDouble { get; set; }
    }

    public class res : IConnectionStringResolver
    {
        string IConnectionStringResolver.ResolveConnectionStringForDataContext(Type[] entityTypes)
        {
            return "Data Source=localhost;Initial Catalog=Test;Integrated Security=True;";
        }
    }

    class ProgramTest
    {
        public static void Run()
        {
            RunTestDecimal();
            RunTestToDouble();
        }

        public static void RunTestDecimal()
        {
            var d = new ccc();
            var ddd = d.TestDecimal.ToList();

            var newList = new TestDecimal[] {
                new TestDecimal() { DecimalValueNotNullable = 10.1234567890123456789m },
                new TestDecimal() { DecimalValueNullable = 10.1234567890123456789m }
            };
            d.TestDecimal.Add(newList);
            d.SaveChanges();
        }

        public static void RunTestToDouble()
        {
            var d = new ccc();
            var query1 = d.TestToDouble.Select(x => new { x });
            var query1Test = query1.ToSql();
            var query2 = d.TestToDouble.Select(x => new { x, val = Convert.ToDouble(x.value.Replace(",", ".")) });
            var query2Test = query2.ToSql();
            var ddd = query2.ToList();
        }
    }
}
