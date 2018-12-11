using System.Linq;
using IdentityServer4.Ids3DataMigration.CustomDal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace IdentityServer4.Ids3DataMigration.Tools
{
    public partial class DataMigrationTool
    {
        public partial class Ids4
        {
            private readonly IConfigurationRoot _configuration;

            //DbUsageStrategy.UseMultipleDb:

            //private string Ids4ClientsConnectiionString =>
            //    @"Data Source=(LocalDb)\\MSSQLLocalDB;database=IdentityServer4.Configuration;trusted_connection=yes;";

            //private string Ids4OperationalStoreConnectiionString =>
            //    @"Data Source=(LocalDb)\\MSSQLLocalDB;database=IdentityServer4.OperationalStore;trusted_connection=yes;";


            public Ids4(IConfigurationRoot configuration)
            {
                _configuration = configuration;
            }


            public Ids4RootDTO GetIds4ClientsRoot()
            {
                using (CustomConfigurationDbContext clientCtx = new CustomConfigurationDbContextFactory(_configuration).CreateDbContext(null))
                {
                    var clients = clientCtx.Clients
                        .Include(x => x.Claims)
                        .Include(x => x.AllowedCorsOrigins)
                        .Include(x => x.ClientSecrets)
                        .Include(x => x.RedirectUris)
                        .Include(x => x.PostLogoutRedirectUris)
                        .Include(x => x.AllowedScopes)
                        //.ThenInclude(x=>x.ChildCollection)
                        .Include(x => x.IdentityProviderRestrictions)
                        .Include(x => x.AllowedGrantTypes)
                        .Include(x => x.Properties)

                        .AsNoTracking().ToArray();


                    var apiResources = clientCtx.ApiResources
                        .Include(x => x.Scopes)
                        .ThenInclude(z=>z.UserClaims)

                        .Include(x => x.UserClaims)
                        .Include(x => x.Secrets)
                        .AsNoTracking().ToArray();

                    var identityResources = clientCtx.IdentityResources
                        .Include(x => x.UserClaims)
                        .AsNoTracking().ToArray();

                    return new Ids4RootDTO
                    {
                        Clients = clients,
                        ApiResources = apiResources,
                        IdentityResources = identityResources
                    };

                }
            }

        }
    }
}