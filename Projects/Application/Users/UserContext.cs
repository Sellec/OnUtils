using System;

namespace OnUtils.Application.Users
{
    using Application.Users;
    using Architecture.AppCore;

    class UserContext : CoreComponentBase<ApplicationCore>, IUserContext
    {
        private Guid _userID = Guid.Empty;
        private bool _isAuthorized;
        private DB.User _data;
        private UserPermissions _permissions;

        public UserContext(DB.User data, bool isAuthorized)
        {
            _userID = GuidIdentifierGenerator.GenerateGuid(GuidType.User, data.IdUser);
            _data = data;
            _isAuthorized = isAuthorized;
            _permissions = new UserPermissions();
        }

        public void ApplyPermissions(UserPermissions permissions = null)
        {
            _permissions = permissions ?? new UserPermissions();
        }

        #region CoreComponentBase
        protected sealed override void OnStart()
        {
        }

        protected sealed override void OnStop()
        {
        }
        #endregion

        public DB.User GetData()
        {
            return _data;
        }

        #region Свойства
        public int IdUser
        {
            get => !_isAuthorized ? 0 : this._data.IdUser;
        }

        Guid IUserContext.UserID
        {
            get => _userID;
        }

        bool IUserContext.IsGuest
        {
            get => !_isAuthorized;
        }

        bool IUserContext.IsSuperuser
        {
            get => _isAuthorized && _data.IsSuperuser;
        }

        UserPermissions IUserContext.Permissions
        {
            get => _permissions;
        }
        #endregion
    }
}
