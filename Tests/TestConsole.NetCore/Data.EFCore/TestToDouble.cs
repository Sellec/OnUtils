using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestConsole.NetCore.Data.EFCore
{
    [Table("TestToDouble")]
    public class TestToDouble
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [MaxLength]
        public string value { get; set; }
    }


}
