using System.Data.Entity;
using System.Linq;
using IdentityServer3.EntityFramework;
using Microsoft.Extensions.Configuration;

namespace IdentityServer4.Ids3DataMigration.Tools
{
    public partial class DataMigrationTool
    {
        public class Ids3
        {
            private readonly string Ids3ConnectiionString;// => @"Data Source=(LocalDb)\MSSQLLocalDb;Initial Catalog=IdentityServer3;Integrated Security=True";

            public Ids3(IConfiguration configuration)
            {
                Ids3ConnectiionString = configuration.GetConnectionString("Ids3.SourceDbConnection");
            }
            /// Using EF6
            public Ids3RootDTO GetIds3ClientsRoot()
            {
                using (ClientConfigurationDbContext clientCtx = new ClientConfigurationDbContext(Ids3ConnectiionString))
                {
                    var clients = clientCtx.Clients
                        .Include(x => x.Claims)
                        .Include(x => x.AllowedCorsOrigins)
                        .Include(x => x.AllowedCustomGrantTypes)
                        .Include(x => x.ClientSecrets)
                        .Include(x => x.RedirectUris)
                        .Include(x => x.PostLogoutRedirectUris)
                        .Include(x => x.AllowedScopes)
                        .Include(x => x.IdentityProviderRestrictions)
                        .Include(x => x.AllowedCustomGrantTypes)
                        //.Include(x => x.Properties)

                        .AsNoTracking().ToArray();

                    // Assume that App uses 3 contexts in the same database
                    using (ScopeConfigurationDbContext scopeCtx = new ScopeConfigurationDbContext(Ids3ConnectiionString))
                    {
                        var scopes = scopeCtx.Scopes
                            .Include(x => x.ScopeClaims)
                            .Include(x => x.ScopeSecrets)

                            .AsNoTracking().ToArray();

                        return new Ids3RootDTO
                        {
                            Clients = clients,
                            Scopes = scopes
                        };

                    }
                }
            }
        }
    }
}
