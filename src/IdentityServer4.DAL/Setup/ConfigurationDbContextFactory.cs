using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IdentityServer4.DAL.Setup
{
    public class ConfigurationDbContextFactory : DbContextFactoryBase, IDesignTimeDbContextFactory<ConfigurationDbContext>
    {
        public ConfigurationDbContextFactory() : base(AppSettingsMockingBuilder.BuildConfiguration("Development"))
        {
            _schemaConnectionKey = "ConfigurationDbConnection";
        }

        public ConfigurationDbContextFactory(IConfigurationRoot configuration):base(configuration)
        {
            _schemaConnectionKey = "ConfigurationDbConnection";
        }

        public ConfigurationDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ConfigurationDbContext>();
            builder.UseSqlServer(_config.GetConnectionString(_connectionName), b => b.MigrationsAssembly("IdentityServer4.DAL"));
            ConfigurationStoreOptions options = new ConfigurationStoreOptions()
            {
            };

            return new ConfigurationDbContext(builder.Options, options)
            {

            };
        }
    }
}