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
        public IRepository<TestUpsert> TestUpsert { get; set; }
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
            RunTestUpsert();
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

        public static void RunTestUpsert()
        {
            var item1 = new TestUpsert() { Id = 1, UniqueKey = "item1", value = DateTime.Now };
            var item2 = new TestUpsert() { Id = 0, UniqueKey = "item2", value = DateTime.Now };
            var item3 = new TestUpsert() { Id = 3, UniqueKey = "item3", value = DateTime.Now };

            var d = new ccc();
            d.TestUpsert.GetDbSet().Upsert(item1).On(x => x.UniqueKey).WhenMatched((xDb, xIns) => new TestUpsert() { value = xIns.value }).Run();
            d.TestUpsert.GetDbSet().Upsert(item2).On(x => x.UniqueKey).WhenMatched((xDb, xIns) => new TestUpsert() { value = xIns.value }).Run();
            d.TestUpsert.GetDbSet().Upsert(item3).On(x => x.UniqueKey).WhenMatched((xDb, xIns) => new TestUpsert() { value = xIns.value }).Run();

            var ddd = d.TestUpsert.ToList();
        }
    }
}
