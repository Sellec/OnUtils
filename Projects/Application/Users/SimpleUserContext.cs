using System;
using System.Collections.Generic;
using System.Text;

namespace OnUtils.Application.Users
{
    using Architecture.AppCore;

    class SimpleUserContext<TApplication> : CoreComponentBase<TApplication>, IUserContext
        where TApplication : AppCore<TApplication>
    {
        public Guid UserID { get; set; }

        public bool IsGuest { get; set; }

        public bool IsSuperuser { get; set; }

        public PermissionsList Permissions { get; set; } = new PermissionsList();

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }
    }
}
