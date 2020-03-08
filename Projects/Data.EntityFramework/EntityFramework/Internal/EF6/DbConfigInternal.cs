using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace OnUtils.Data.EntityFramework.Internal
{
    class DbConfigInternal : DbConfiguration
    {
        public DbConfigInternal()
        {
            SetManifestTokenResolver(new MyManifestTokenResolver());
        }

        public class MyManifestTokenResolver : IManifestTokenResolver
        {
            private readonly IManifestTokenResolver _defaultResolver = new DefaultManifestTokenResolver();

            string IManifestTokenResolver.ResolveManifestToken(DbConnection connection)
            {
                var sqlConn = connection as SqlConnection;
                if (sqlConn != null)
                {
                    return "2008";
                }
                else
                {
                    return _defaultResolver.ResolveManifestToken(connection);
                }
            }

        }
    }


}
