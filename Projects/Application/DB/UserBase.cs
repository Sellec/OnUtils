namespace OnUtils.Application.DB
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS1591 // todo внести комментарии.
    [Table("UserBase")]
    public partial class UserBase
    {
        public UserBase()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdUser { get; set; }

        public bool IsSuperuser { get; set; }

        public DateTime DateChange { get; set; }

        public int IdUserChange { get; set; }

        [StringLength(200)]
        public string UniqueKey { get; set; }

    }
}
