using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoMapper;
using IdentityServer3.Core.Models;
using IdentityServer4.DAL.Seed;
using IdentityServer4.DAL.Seed.Development;
using IdentityServer4.DAL.Setup;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Ids3DataMigration.CustomDal;
using IdentityServer4.Ids3DataMigration.Mapping.Profiles;
using IdentityServer4.Ids3DataMigration.Mapping.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IdentityServer4.Ids3DataMigration.Tools
{
    using Ids4Entities = EntityFramework.Entities;


    public partial class DataMigrationTool
    {
        public DataMigrationTool.Ids3 Ids3Tool;
        public DataMigrationTool.Ids4 Ids4Tool;

        private readonly IConfigurationRoot _configuration;
        private readonly IMapper _mapper;

        public DataMigrationTool(IConfigurationRoot configuration)
        {
            _configuration = configuration;
            Ids3Tool = new DataMigrationTool.Ids3(configuration);
            Ids4Tool = new DataMigrationTool.Ids4(configuration);

            _mapper = MapperFactory.CreateMapper<Ids3ToIds4EntityProfile>();
            // zero of unmapped properties expected:
            TestMapperProfile<Ids3ToIds4EntityProfile>(0);
        }

        private static void TestMapperProfile<T>(int expectedCount = 0) where T : Profile, new()
        {
            var mapper = MapperFactory.CreateMapper<T>();
            var unmappedProperties = mapper.GetUnmappedSimpleProperties();
            Assert.True(unmappedProperties.Count == expectedCount, unmappedProperties.GetMessage(typeof(T)));
        }

        /// Read and Map. Not saving changes to Db
        public (Ids3RootDTO, Ids4RootDTO) ReadIds3DbAndMapClientsScopesTreeToIds4Schema(bool enableScopeToApiResource2ndLevelMapping)
        {
            var clients3Source = Ids3Tool.GetIds3ClientsRoot();
            var clients4Target = _mapper.Map<Ids4Entities.Client[]>(clients3Source.Clients);

            //  IdentityResources
            var identityResources4Target = _mapper.Map<Ids4Entities.IdentityResource[]>(clients3Source.Scopes.Where(x => x.Type == (int)ScopeType.Identity));

            //  ApiResources
            var apiResources4Target = _mapper.Map<Ids4Entities.ApiResource[]>(clients3Source.Scopes.Where(x => x.Type == (int)ScopeType.Resource));

            var claims = apiResources4Target.SelectMany(x => x.UserClaims)
                .Select(x => new
                {
                    x.Type,
                    x.ApiResourceId,
                    ApiResourceName = x.ApiResource?.Name
                }).ToList();

            claims.ForEach(x=>Debug.WriteLine($"{x.Type}: ({x.ApiResourceId}-{x.ApiResourceName})")); 


            var resourcesStorage = new ResourcesDataStorage(
                identityResources4Target, apiResources4Target,
                new List<string>(),// don't have any existing in Ids4 Identity => don't care
                new List<string>());// don't have any Resource n Ids4 => don't care

            foreach (var resourceClaim in resourcesStorage.ApiResourceClaims)
            {
                var t = resourceClaim.Type;
            }

            if (enableScopeToApiResource2ndLevelMapping)
            {
                var apiScopes =
                    _mapper.Map<Ids4Entities.ApiScope[]>(
                        clients3Source.Scopes.Where(x => x.Type == (int)ScopeType.Resource));

                // Transform children Level#1 and Level#2
                for (int i = 0; i < apiResources4Target.Length; i++)
                {
                    var ar = apiResources4Target[i];
                    apiScopes[i].ApiResource = ar;

                    // 1 ApiResource => 1 ApiScope
                    ar.Scopes.Add(apiScopes[i]);

                    for (int j = 0; j < apiScopes[i].UserClaims.Count; j++)
                    {
                        apiScopes[i].UserClaims[j].ApiScope = apiScopes[i];
                        apiScopes[i].UserClaims[j].ApiScopeId = apiScopes[i].Id;
                    }
                }
            }

            return (clients3Source, new Ids4RootDTO
            {
                Clients = clients4Target,
                IdentityResources = identityResources4Target,
                ApiResources = apiResources4Target
            });
        }

        /// Making Ids4 entities tree from Ids3 and Copy to Ids4 Database
        public (Ids3RootDTO, Ids4RootDTO) CopyClientsScopesTreeFromIds3DbToIds4Db(bool enableScopeToApiResource2ndLevelMapping)
        {
            var existingTargetClients4 = Ids4Tool.GetIds4ClientsRoot();
            var existingClients4Ids = existingTargetClients4.Clients.Select(x => x.ClientId).ToList();

            // Source
            var clients3Source = Ids3Tool.GetIds3ClientsRoot();
            var clients = _mapper.Map<Ids4Entities.Client[]>(clients3Source.Clients);

            var storage = new ClientDataStorage(clients.ToList(), existingClients4Ids);

            using (CustomConfigurationDbContext ctx = new CustomConfigurationDbContextFactory(_configuration).CreateDbContext(Array.Empty<string>()))
            {
                DatabaseHelper.SwitchIdentityInsertState(ctx, "OFF");

                // Clients
                foreach (var c in storage.Clients)
                {
                    // Add to DbContext
                    ctx.Clients.Add(c);
                }

                storage.Filter(existingClients4Ids);// optional double check, just for sure
                SaveClientsWithChildren(ctx, storage);


                //  IdentityResources
                var identityResources4Target = _mapper.Map<Ids4Entities.IdentityResource[]>(clients3Source.Scopes.Where(x => x.Type == (int)ScopeType.Identity));

                //  ApiResources
                var apiResources4Target = _mapper.Map<Ids4Entities.ApiResource[]>(clients3Source.Scopes.Where(x => x.Type == (int)ScopeType.Resource));

                // TODO: think about 2nd level claims and rework properly
                // because 2nd level api scope includes into aud claim
                if (enableScopeToApiResource2ndLevelMapping)
                {
                    var apiScopes =
                        _mapper.Map<Ids4Entities.ApiScope[]>(
                            clients3Source.Scopes.Where(x => x.Type == (int) ScopeType.Resource));

                    // Transform children Level#1 and Level#2
                    for (int i = 0; i < apiResources4Target.Length; i++)
                    {
                        var ar = apiResources4Target[i];
                        apiScopes[i].ApiResource = ar;

                        // 1 ApiResource => 1 ApiScope
                        ar.Scopes.Add(apiScopes[i]);

                        for (int j = 0; j < apiScopes[i].UserClaims.Count; j++)
                        {
                            apiScopes[i].UserClaims[j].ApiScope = apiScopes[i];
                            apiScopes[i].UserClaims[j].ApiScopeId = apiScopes[i].Id;
                        }
                    }
                }

                var existingApiResNames = existingTargetClients4.ApiResources.Select(x => x.Name).ToList();
                var existingIdentityResNames = existingTargetClients4.IdentityResources.Select(x => x.Name).ToList();

                var resourcesStorage = new ResourcesDataStorage(
                    identityResources4Target, apiResources4Target,
                    existingIdentityResNames,
                    existingApiResNames);

                SaveResources(ctx, resourcesStorage);

                DatabaseHelper.SwitchIdentityInsertState(ctx, "OFF");

                return (clients3Source, new Ids4RootDTO
                {
                    Clients = clients,
                    IdentityResources = identityResources4Target,
                    ApiResources = apiResources4Target
                });
            }
        }

        private void SaveResources(CustomConfigurationDbContext ctx, ResourcesDataStorage storage)
        {
            // ApiResources
            ctx.ApiResources.AddRange(storage.ApiResources);
            SaveWithIdentityColumn(ctx, nameof(ctx.ApiResources));

            //1st level
            ctx.ApiClaims.AddRange(storage.ApiResourceClaims);
            SaveWithIdentityColumn(ctx, nameof(ctx.ApiClaims));

            ctx.ApiSecrets.AddRange(storage.ApiSecrets);
            SaveWithIdentityColumn(ctx, nameof(ctx.ApiSecrets));

            //2nd level
            ctx.ApiScopes.AddRange(storage.ApiScopes);
            SaveWithIdentityColumn(ctx, nameof(ctx.ApiScopes));

            ctx.ApiScopeClaims.AddRange(storage.ApiScopeClaims);
            SaveWithIdentityColumn(ctx, nameof(ctx.ApiScopeClaims));



            // IdentityResources
            ctx.IdentityResources.AddRange(storage.IdentityResources);
            SaveWithIdentityColumn(ctx, nameof(ctx.IdentityResources));

            ctx.IdentityClaims.AddRange(storage.IdentityClaims);
            SaveWithIdentityColumn(ctx, nameof(ctx.IdentityClaims));

        }

        private void SaveClientsWithChildren(CustomConfigurationDbContext ctx, ClientDataStorage storage)
        {
            // Clients
            SaveWithIdentityColumn(ctx, nameof(ctx.Clients));

            // ClientClaims
            ctx.ClientClaims.AddRange(storage.Claims);
            SaveWithIdentityColumn(ctx, nameof(ctx.ClientClaims));

            // ClientRedirectUris
            ctx.ClientRedirectUris.AddRange(storage.RedirectUris);
            SaveWithIdentityColumn(ctx, nameof(ctx.ClientRedirectUris));

            // ClientCorsOrigins
            ctx.ClientCorsOrigins.AddRange(storage.AllowedCorsOrigins);
            SaveWithIdentityColumn(ctx, nameof(ctx.ClientCorsOrigins));

            // ClientScopes
            ctx.ClientScopes.AddRange(storage.AllowedScopes);
            SaveWithIdentityColumn(ctx, nameof(ctx.ClientScopes));

            // AllowedGrantTypes
            ctx.ClientGrantTypes.AddRange(storage.AllowedGrantTypes);
            SaveWithIdentityColumn(ctx, nameof(ctx.ClientGrantTypes));

            // ClientSecrets
            ctx.ClientSecrets.AddRange(storage.ClientSecrets);
            SaveWithIdentityColumn(ctx, nameof(ctx.ClientSecrets));

            // Properties
            ctx.ClientProperties.AddRange(storage.Properties);
            SaveWithIdentityColumn(ctx, nameof(ctx.ClientProperties));

            // IdentityProviderRestrictions
            ctx.ClientIdPRestrictions.AddRange(storage.IdentityProviderRestrictions);
            SaveWithIdentityColumn(ctx, nameof(ctx.ClientIdPRestrictions));
        }


        private void SaveWithIdentityColumn(CustomConfigurationDbContext ctx, string tableName)
        {
            // https://docs.microsoft.com/en-us/ef/core/saving/explicit-values-generated-properties
            ctx.Database.OpenConnection();
            try
            {
#pragma warning disable EF1000 // Possible SQL injection vulnerability.
                ctx.Database.ExecuteSqlCommand(new RawSqlString($"SET IDENTITY_INSERT dbo.{tableName} ON"));
                ctx.SaveChanges();
                ctx.Database.ExecuteSqlCommand(new RawSqlString($"SET IDENTITY_INSERT dbo.{tableName} OFF"));
#pragma warning restore EF1000 // Possible SQL injection vulnerability.

            }
            finally
            {
                ctx.Database.CloseConnection();
            }
        }

        public void CleanUpIds4Db()
        {
            using (CustomConfigurationDbContext ctx = new CustomConfigurationDbContextFactory(_configuration)
                .CreateDbContext(Array.Empty<string>()))
            {
                DataSeeder.CleanUpConfigurationDb(ctx);
            }

        }
        public void SeedAdminClient(IClientsConfigData clientsConfig)
        {
            // Damned hack for ConfigurationDbContextFactory!
            DbConnectionSwitcher.SingleDbConnectionKey = "Ids4.TargetDbConnection";

            using (ConfigurationDbContext ctx = new ConfigurationDbContextFactory(_configuration)
                .CreateDbContext(Array.Empty<string>()))
            {
                var adminClient = clientsConfig.GetClients()
                    .First(x => x.ClientName == AdminPortalConsts.OidcClientId);

                ctx.Clients.Add(adminClient.ToEntity());
                ctx.SaveChanges();
            }
        }

        public void SeedClientsDataToContextIfEmpty(IClientsConfigData clientsConfig)
        {
            // Damned hack for ConfigurationDbContextFactory!
            DbConnectionSwitcher.SingleDbConnectionKey = "Ids4.TargetDbConnection";

            using (ConfigurationDbContext context = new ConfigurationDbContextFactory(_configuration)
                .CreateDbContext(Array.Empty<string>()))
            {

                // Clients
                if (!context.Clients.Any())
                {
                    Console.WriteLine("Clients being populated");

                    foreach (var client in clientsConfig.GetClients().ToList())
                    {
                        context.Clients.Add(client.ToEntity());
                    }

                    context.SaveChanges();
                }
                else
                {
                    Console.WriteLine("Clients already populated");
                }

                // IdentityResources
                if (!context.IdentityResources.Any())
                {
                    Console.WriteLine("IdentityResources being populated");
                    foreach (var resource in clientsConfig.GetIdentityResources().ToList())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }

                    context.SaveChanges();
                }
                else
                {
                    Console.WriteLine("IdentityResources already populated");
                }

                // ApiResources
                if (!context.ApiResources.Any())
                {
                    Console.WriteLine("ApiResources being populated");
                    foreach (var resource in clientsConfig.GetApiResources().ToList())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }

                    context.SaveChanges();
                }
                else
                {
                    Console.WriteLine("ApiResources already populated");
                }
            }
        }

    }
}
