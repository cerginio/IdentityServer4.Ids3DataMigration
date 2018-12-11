using Microsoft.Extensions.Configuration;

namespace IdentityServer4.DAL.Setup
{
    public abstract class DbContextFactoryBase
    {
        protected readonly IConfigurationRoot _config;
        protected readonly string _connectionName;
        protected string _schemaConnectionKey = "Ids4DbConnection";// Ids4DbConnection | ConfigurationDbConnection | OperationalStoreDbConnection

        protected DbContextFactoryBase(IConfigurationRoot config)
        {
            _config = config;

            if (_config == null)
                _config = AppSettingsMockingBuilder.BuildConfiguration("Development");

            _connectionName = DbConnectionSwitcher.SwitchConnectionKey(_config, _schemaConnectionKey);
        }

    }
}