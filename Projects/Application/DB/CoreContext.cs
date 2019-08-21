namespace OnUtils.Application.DB
{
    using Data;

#pragma warning disable CS1591 // todo внести комментарии.
    public abstract class CoreContextBase : UnitOfWorkBase
    {

    }

    public class CoreContext : CoreContextBase
    {
        public IRepository<ModuleConfig> Module { get; }

        public IRepository<ItemParent> ItemParent { get; }
        public IRepository<ItemType> ItemType { get; }
        
        public IRepository<UserEntity> UserEntity { get; }
        public IRepository<UserBase> Users { get; }

        public IRepository<Role> Role { get; }
        public IRepository<RoleUser> RoleUser { get; }
        public IRepository<RolePermission> RolePermission { get; }
    }
}
