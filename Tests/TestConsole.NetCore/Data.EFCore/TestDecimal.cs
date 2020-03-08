using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestConsole.NetCore.Data.EFCore
{
    [Table("TestDecimal")]
    public class TestDecimal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdKey { get; set; }

        [DecimalPrecision(5, 2)]
        public decimal? DecimalValueNullable { get; set; }

        public decimal DecimalValueNotNullable { get; set; }
    }


}
