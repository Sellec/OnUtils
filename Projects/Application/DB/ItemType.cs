namespace OnUtils.Application.DB
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS1591 // todo внести комментарии.
    [Table("ItemType")]
    public partial class ItemType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdItemType { get; set; }

        [Required]
        [StringLength(200)]
        public string NameItemType { get; set; }

        [Required]
        [StringLength(200)]
        public string UniqueKey { get; set; }

        public static explicit operator int(ItemType type)
        {
            return type == null ? 0 : type.IdItemType;
        }

        public static explicit operator ItemType(int idItemType)
        {
            return Items.ItemTypeFactory.GetItemType(idItemType);
        }
    }
}
