using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestConsole.NetCore.Data.EFCore
{
    [Table("TestUpsert")]
    public class TestUpsert
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string UniqueKey { get; set; }

        public DateTime value { get; set; }
    }


}
