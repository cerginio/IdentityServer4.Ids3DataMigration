
using IdentityServer3.EntityFramework.Entities;

namespace IdentityServer4.Ids3DataMigration.Tools
{
    public class Ids3RootDTO
    {
        public Client[] Clients { get; set; }
        public Scope[] Scopes { get; set; }

    }
}