using Microsoft.Extensions.Configuration;

namespace IdentityServer4.DAL.Setup
{
    /// Aim of that helper to switch connection strings from specific to single according
    /// DbUsageStrategy: UseSingleDb | UseMultipleDb
    public static class DbConnectionSwitcher
    {
        public static string SingleDbConnectionKey = nameof(Ids4ConnectionStrings.ConfigurationDbConnection);

       
        public static string SwitchConnection(Ids4ConnectionStrings connectionStrings, string connStringKey)
        {
            return connectionStrings.DbUsageStrategy == DbUsageStrategy.UseSingleDb
                ? connectionStrings.ConfigurationDbConnection
                : connStringKey;
        }

        public static string SwitchConnectionKey(IConfigurationRoot configuration, string connStringKey)
        {
            var dbUsageStrategy = configuration.GetConnectionString(nameof(DbUsageStrategy)) ?? DbUsageStrategy.UseSingleDb;

            return dbUsageStrategy == DbUsageStrategy.UseSingleDb
                ? SingleDbConnectionKey
                : connStringKey;
        }
      
    }

    public static class DbUsageStrategy
    {
        public const string UseMultipleDb = "UseMultipleDb";

        // Default
        public const string UseSingleDb = "UseSingleDb";

    }

    public interface Ids4ConnectionStrings
    {
        /// If Merge ConfigurationDb,OperationalStoreDb,IdentityDb schemas into one db
        /// Setting introduced due to Skoruba Admin tool implementation,
        /// which operates by single AdminDbContext
        string DbUsageStrategy { get; set; }

        string ConfigurationDbConnection { get; set; }

        string OperationalStoreDbConnection { get; set; }

    }
}