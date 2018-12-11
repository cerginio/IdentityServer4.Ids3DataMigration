using System;
using System.Threading.Tasks;
using IdentityServer4.DAL.Setup;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IdentityServer4.Ids3DataMigration.CustomDal
{
    public class CustomConfigurationDbContextFactory : DbContextFactoryBase, IDesignTimeDbContextFactory<CustomConfigurationDbContext>
    {
        private const string ConfigurationDbConnection = "Ids4.TargetDbConnection";

        static CustomConfigurationDbContextFactory()
        {
            DbConnectionSwitcher.SingleDbConnectionKey = "Ids4.TargetDbConnection";
        }

        public CustomConfigurationDbContextFactory(IConfigurationRoot configuration) : base(configuration)
        {
            _schemaConnectionKey = ConfigurationDbConnection;
        }

        public CustomConfigurationDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<CustomConfigurationDbContext>();

            builder.UseSqlServer(_config.GetConnectionString(_connectionName), b => b.MigrationsAssembly("IdentityServer4.DAL"));
            ConfigurationStoreOptions storeOptions = new ConfigurationStoreOptions();

            var ctx = new CustomConfigurationDbContext(builder.Options, storeOptions);
            return ctx;
        }
    }

    /// <summary>
    /// DbContext for the IdentityServer configuration data.
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
    /// <seealso cref="IdentityServer4.EntityFramework.Interfaces.IConfigurationDbContext" />
    public class CustomConfigurationDbContext : CustomConfigurationDbContext<CustomConfigurationDbContext>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityServer4.EntityFramework.DbContexts.ConfigurationDbContext"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="storeOptions">The store options.</param>
        /// <exception cref="ArgumentNullException">storeOptions</exception>
        public CustomConfigurationDbContext(DbContextOptions<CustomConfigurationDbContext> options, ConfigurationStoreOptions storeOptions)
            : base(options, storeOptions)
        {
        }

    }

    /// <summary>
    /// DbContext for the IdentityServer configuration data.
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
    /// <seealso cref="IdentityServer4.EntityFramework.Interfaces.IConfigurationDbContext" />
    public class CustomConfigurationDbContext<TContext> : DbContext, IConfigurationDbContext
        where TContext : DbContext, IConfigurationDbContext
    {
        private readonly ConfigurationStoreOptions storeOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityServer4.EntityFramework.DbContexts.ConfigurationDbContext"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="storeOptions">The store options.</param>
        /// <exception cref="ArgumentNullException">storeOptions</exception>
        public CustomConfigurationDbContext(DbContextOptions<TContext> options, ConfigurationStoreOptions storeOptions)
            : base(options)
        {
            this.storeOptions = storeOptions ?? throw new ArgumentNullException(nameof(storeOptions));
        }

        /// <summary>
        /// Gets or sets the clients.
        /// </summary>
        /// <value>
        /// The clients.
        /// </value>
        public DbSet<Client> Clients { get; set; }

        /// Custom DbSet for Insert
        /// Client
        public DbSet<ClientClaim> ClientClaims { get; set; }
        public DbSet<ClientRedirectUri> ClientRedirectUris { get; set; }
        public DbSet<ClientCorsOrigin> ClientCorsOrigins { get; set; }
        public DbSet<ClientScope> ClientScopes { get; set; }
        public DbSet<ClientSecret> ClientSecrets { get; internal set; }
        public DbSet<ClientGrantType> ClientGrantTypes { get; internal set; }
        public DbSet<ClientIdPRestriction> ClientIdPRestrictions { get; set; }
        public DbSet<ClientProperty> ClientProperties { get; set; }


        /// Resources

        /// <summary>
        /// Gets or sets the identity resources.
        /// </summary>
        /// <value>
        /// The identity resources.
        /// </value>
        public DbSet<IdentityResource> IdentityResources { get; set; }

        // IdentityResources.Children
        public DbSet<IdentityClaim> IdentityClaims { get; set; }

        /// <summary>
        /// Gets or sets the API resources.
        /// </summary>
        /// <value>
        /// The API resources.
        /// </value>
        public DbSet<ApiResource> ApiResources { get; set; }

        // ApiResources.Children
        public DbSet<ApiResourceClaim> ApiClaims { get; set; } // => 1st level ApiResourceClaims

        public DbSet<ApiScope> ApiScopes { get; set; }// 2nd level
        public DbSet<ApiScopeClaim> ApiScopeClaims { get; set; }// 2nd level
        public DbSet<ApiSecret> ApiSecrets { get; set; }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <returns></returns>
        public Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }

        /// <summary>
        /// Override this method to further configure the model that was discovered by convention from the entity types
        /// exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting model may be cached
        /// and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context. Databases (and other extensions) typically
        /// define extension methods on this object that allow you to configure aspects of the model that are specific
        /// to a given database.</param>
        /// <remarks>
        /// If a model is explicitly set on the options for this context (via <see cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />)
        /// then this method will not be run.
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // (SK) Custom
            modelBuilder.CustomConfigureClientContext(storeOptions);
            modelBuilder.CustomConfigureResourcesContext(storeOptions);

            base.OnModelCreating(modelBuilder);
        }
    }
}
