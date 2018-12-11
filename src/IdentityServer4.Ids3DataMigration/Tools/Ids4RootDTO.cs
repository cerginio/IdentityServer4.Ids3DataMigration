

using IdentityServer4.EntityFramework.Entities;

namespace IdentityServer4.Ids3DataMigration.Tools
{
    public class Ids4RootDTO
    {
        public Client[] Clients { get; set; }
        public IdentityResource[] IdentityResources { get; set; }
        public ApiResource[] ApiResources { get; set; }



    }
}