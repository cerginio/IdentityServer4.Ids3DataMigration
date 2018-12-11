using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IdentityServer4.DAL.Setup
{
    public class PersistanceGrantDbContextFactory : DbContextFactoryBase, IDesignTimeDbContextFactory<PersistedGrantDbContext>
    {
        public PersistanceGrantDbContextFactory() : base(AppSettingsMockingBuilder.BuildConfiguration("Development"))
        {
            _schemaConnectionKey = "OperationalStoreDbConnection";
        }

        public PersistanceGrantDbContextFactory(IConfigurationRoot configuration) : base(configuration)
        {
            _schemaConnectionKey = "OperationalStoreDbConnection";
        }

        public PersistedGrantDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<PersistedGrantDbContext>();
            builder.UseSqlServer(_config.GetConnectionString(_connectionName), b => b.MigrationsAssembly("IdentityServer4.DAL"));

            OperationalStoreOptions options = new OperationalStoreOptions()
            {

            };

            return new PersistedGrantDbContext(builder.Options, options);
        }
    }
}
