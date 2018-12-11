using System;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer4.Ids3DataMigration.Tools
{
    using Ids4Entities = EntityFramework.Entities;

    public class ClientDataStorage
    {
        public ClientDataStorage(List<Ids4Entities.Client> clients, List<string> existingClientIds)
        {
            Clients = clients.Where(x=> !existingClientIds.Contains(x.ClientId)).ToList();

            ClientSecrets = new List<Ids4Entities.ClientSecret>();
            AllowedGrantTypes = new List<Ids4Entities.ClientGrantType>();
            RedirectUris = new List<Ids4Entities.ClientRedirectUri>();
            PostLogoutRedirectUris = new List<Ids4Entities.ClientPostLogoutRedirectUri>();
            AllowedScopes = new List<Ids4Entities.ClientScope>();
            IdentityProviderRestrictions = new List<Ids4Entities.ClientIdPRestriction>();
            Claims = new List<Ids4Entities.ClientClaim>();
            AllowedCorsOrigins = new List<Ids4Entities.ClientCorsOrigin>();
            Properties = new List<Ids4Entities.ClientProperty>();

            // Clients
            foreach (var c in Clients)
            {
                if (existingClientIds.Contains(c.ClientId))
                {
                    // Interrupt operation of client copy if exists
                    throw new InvalidOperationException($"Client already exists: {c.ClientId}-{c.ClientName}");
                }

                // Store Client Navigation properties
                Claims.AddRange(c.Claims ?? Array.Empty<Ids4Entities.ClientClaim>().ToList());
                AllowedCorsOrigins.AddRange(c.AllowedCorsOrigins ?? Array.Empty<Ids4Entities.ClientCorsOrigin>().ToList());
                AllowedScopes.AddRange(c.AllowedScopes ?? Array.Empty<Ids4Entities.ClientScope>().ToList());

                AllowedGrantTypes.AddRange(c.AllowedGrantTypes ?? Array.Empty<Ids4Entities.ClientGrantType>().ToList());
                ClientSecrets.AddRange(c.ClientSecrets ?? Array.Empty<Ids4Entities.ClientSecret>().ToList());
                RedirectUris.AddRange(c.RedirectUris ?? Array.Empty<Ids4Entities.ClientRedirectUri>().ToList());
                PostLogoutRedirectUris.AddRange(c.PostLogoutRedirectUris ?? Array.Empty<Ids4Entities.ClientPostLogoutRedirectUri>().ToList());
                IdentityProviderRestrictions.AddRange(c.IdentityProviderRestrictions ?? Array.Empty<Ids4Entities.ClientIdPRestriction>().ToList());

                Properties.AddRange(c.Properties ?? Array.Empty<Ids4Entities.ClientProperty>().ToList());

                // Flattering
                //Clean Client Entity children tree to plain DTO
                c.Claims = null;
                c.AllowedCorsOrigins = null;
                c.AllowedScopes = null;
                c.AllowedGrantTypes = null;
                c.ClientSecrets = null;
                c.RedirectUris = null;
                c.PostLogoutRedirectUris = null;
                c.IdentityProviderRestrictions = null;
                c.Properties = null;
            }
        }


        public void Filter(List<string> existingClientIds)
        {
            Clients = Clients.Where(x => !existingClientIds.Contains(x.ClientId)).ToList();

            ClientSecrets = ClientSecrets.Where(x => !existingClientIds.Contains(x.Client.ClientId)).ToList();
            AllowedGrantTypes = AllowedGrantTypes.Where(x => !existingClientIds.Contains(x.Client.ClientId)).ToList();
            RedirectUris = RedirectUris.Where(x => !existingClientIds.Contains(x.Client.ClientId)).ToList();
            PostLogoutRedirectUris = PostLogoutRedirectUris.Where(x => !existingClientIds.Contains(x.Client.ClientId)).ToList();
            AllowedScopes = AllowedScopes.Where(x => !existingClientIds.Contains(x.Client.ClientId)).ToList();
            Claims = Claims.Where(x => !existingClientIds.Contains(x.Client.ClientId)).ToList();
            AllowedCorsOrigins = AllowedCorsOrigins.Where(x => !existingClientIds.Contains(x.Client.ClientId)).ToList();
            IdentityProviderRestrictions = IdentityProviderRestrictions.Where(x => !existingClientIds.Contains(x.Client.ClientId)).ToList();
            Properties = Properties.Where(x => !existingClientIds.Contains(x.Client.ClientId)).ToList();
        }

        // Root: Client
        public List<Ids4Entities.Client> Clients { get; set; }


        // Client children
        public List<Ids4Entities.ClientSecret> ClientSecrets { get; set; }
        public List<Ids4Entities.ClientGrantType> AllowedGrantTypes { get; set; }
        public List<Ids4Entities.ClientRedirectUri> RedirectUris { get; set; }
        public List<Ids4Entities.ClientPostLogoutRedirectUri> PostLogoutRedirectUris { get; set; }
        public List<Ids4Entities.ClientScope> AllowedScopes { get; set; }
        public List<Ids4Entities.ClientClaim> Claims { get; set; }
        public List<Ids4Entities.ClientCorsOrigin> AllowedCorsOrigins { get; set; }

        public List<Ids4Entities.ClientIdPRestriction> IdentityProviderRestrictions { get; set; }
        public List<Ids4Entities.ClientProperty> Properties { get; set; }
    }
}