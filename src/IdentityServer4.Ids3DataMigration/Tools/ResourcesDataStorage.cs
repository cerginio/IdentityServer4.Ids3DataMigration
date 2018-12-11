using System;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer4.Ids3DataMigration.Tools
{
    using Ids4Entities = EntityFramework.Entities;

    public class ResourcesDataStorage
    {
        public ResourcesDataStorage(
            Ids4Entities.IdentityResource[] identityResources,
            Ids4Entities.ApiResource[] apiResources,
            List<string> existingIdentityResNames,
            List<string> existingApiResNames)
        {

            // Filter
            IdentityResources = identityResources.Where(x => !existingIdentityResNames.Contains(x.Name)).ToList();
            ApiResources = apiResources.Where(x => !existingApiResNames.Contains(x.Name)).ToList();

            IdentityClaims = new List<Ids4Entities.IdentityClaim>();

            //
            ApiSecrets = new List<Ids4Entities.ApiSecret>();
            ApiScopes = new List<Ids4Entities.ApiScope>();
            ApiScopeClaims = new List<Ids4Entities.ApiScopeClaim>();
            ApiResourceClaims = new List<Ids4Entities.ApiResourceClaim>();

            foreach (var ar in ApiResources)
            {
                if (existingApiResNames.Contains(ar.Name))
                {
                    throw new InvalidOperationException($"Api resource exists: {ar.Name}");
                }

                // 1st level
                ApiResourceClaims.AddRange(ar.UserClaims ?? Array.Empty<Ids4Entities.ApiResourceClaim>().ToList());
                ApiSecrets.AddRange(ar.Secrets ?? Array.Empty<Ids4Entities.ApiSecret>().ToList());

                // 2nd level
                ApiScopes.AddRange(ar.Scopes ?? Array.Empty<Ids4Entities.ApiScope>().ToList());
                ApiScopeClaims.AddRange(ar.Scopes?.SelectMany(x => x.UserClaims) ?? Array.Empty<Ids4Entities.ApiScopeClaim>().ToList());

                // Flattering
                if (ar.Scopes != null)
                {
                    foreach (var asc in ar.Scopes)
                    {
                        asc.UserClaims = null;
                    }
                }

                ar.UserClaims = null;
                ar.Secrets = null;
                ar.Scopes = null;
            }

            foreach (var ir in IdentityResources)
            {
                if (existingIdentityResNames.Contains(ir.Name))
                {
                    throw new InvalidOperationException($"Identity resource exists: {ir.Name}");
                }

                ir.UserClaims.ForEach(x => x.IdentityResource = ir);

                IdentityClaims.AddRange(ir.UserClaims ?? Array.Empty<Ids4Entities.IdentityClaim>().ToList());

                // Flattering
                ir.UserClaims = null;
            }
        }

        public List<Ids4Entities.IdentityResource> IdentityResources { get; set; }
        public List<Ids4Entities.IdentityClaim> IdentityClaims { get; set; }


        public List<Ids4Entities.ApiResource> ApiResources { get; set; }

        public List<Ids4Entities.ApiSecret> ApiSecrets { get; set; }
        public List<Ids4Entities.ApiScope> ApiScopes { get; set; }


        public List<Ids4Entities.ApiResourceClaim> ApiResourceClaims { get; set; }
        public List<Ids4Entities.ApiScopeClaim> ApiScopeClaims { get; set; }

    }
}