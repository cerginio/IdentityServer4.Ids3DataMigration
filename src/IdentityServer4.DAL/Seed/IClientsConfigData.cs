using System.Collections.Generic;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace IdentityServer4.DAL.Seed
{
    public interface IClientsConfigData
    {
        IEnumerable<IdentityResource> GetIdentityResources();
        IEnumerable<ApiResource> GetApiResources();
        IEnumerable<Client> GetClients();

        // Get TestUsers
        List<TestUser> GetTestUsers();
    }
}