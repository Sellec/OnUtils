namespace OnUtils.Application.DB
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS1591 // todo внести комментарии.
    [Table("User")]
    [Items.ItemType(Modules.ModuleCore.ItemType)]
    public partial class UserBase : Items.ItemBase
    {
        public UserBase() : base(DeprecatedSingletonInstances.ModulesManager.GetModule<Modules.UsersManagement.ModuleUsersManagement>())
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdUser { get; set; }

        public bool IsSuperuser { get; set; }

        public DateTime DateChange { get; set; }

        public int IdUserChange { get; set; }

        [StringLength(200)]
        public string UniqueKey { get; set; }

        #region ItemBase
        /// <summary>
        /// См. <see cref="IdUser"/>.
        /// </summary>
        public override int ID
        {
            get => IdUser;
            set => IdUser = value;
        }

        /// <summary>
        /// </summary>
        public override string Caption
        {
            get => IdUser.ToString();
            set { }
        }

        /// <summary>
        /// Время последнего изменения на основе <see cref="DateChange"/>. 
        /// </summary>
        public override DateTime DateChangeBase
        {
            get => DateChange;
            set => DateChange = value;
        }

        public override Uri Url
        {
            get => null;           
        }
        #endregion
    }
}
