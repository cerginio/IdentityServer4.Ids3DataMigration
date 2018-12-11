using System.Linq;
using IdentityServer4.DAL.Seed.Development;
using IdentityServer4.DAL.Setup;
using IdentityServer4.Ids3DataMigration.Tools;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IdentityServer4.Ids3DataMigration.Tests
{
    [Trait("Category", "DAL")]
    public class DataMigrationToolTest
    {
        private const string EnvironmentName = "alfa-to-local-scenario"; // local-to-local-scenario | alfa-to-beta-scenario

        private IConfigurationRoot Configuration => AppSettingsMockingBuilder.BuildConfiguration(EnvironmentName);

        [Fact]
        public void Ids3_DataMigrationTool_Must_Read_Client_Root()
        {
            var dataIds3Tool = new DataMigrationTool(Configuration).Ids3Tool;

            var res = dataIds3Tool.GetIds3ClientsRoot();

            Assert.NotNull(res.Clients);
            Assert.True(res.Clients.Length > 100);//~190
        }


        [Fact]
        public void Ids4_DataMigrationTool_Must_Read_Client_Root()
        {
            var dataIds4Tool = new DataMigrationTool(Configuration).Ids4Tool;

            var res = dataIds4Tool.GetIds4ClientsRoot();

            Assert.NotNull(res.Clients);
        }


        [Fact]
        public void Read_Client3_And_MapTo_Client4_ProofTest()
        {
            // Arrange
            var tool = new DataMigrationTool(Configuration);

            //Act
            var result = tool.ReadIdsAndMapClientTreetoIds4(true);

            // Assert
            // todo: ApiResourceClaims Scope !!
            var claimsIdSource = result.Item1.Scopes.Where(x => x.Type == 0).SelectMany(x => x.ScopeClaims)
                .Select(x => new
                {
                    x.Name,
                    ScopeName = x.Scope?.Name
                }).ToArray();

            var claimsIdTarget = result.Item2.IdentityResources.Where(x => x.UserClaims != null).SelectMany(x => x.UserClaims)
                .Select(x => new
                {
                    x.Type,
                    x.IdentityResourceId,
                    IdentityResourceName = x.IdentityResource?.Name
                }).ToArray();


            var claimsResSource = result.Item1.Scopes.Where(x => x.Type == 1).SelectMany(x => x.ScopeClaims)
                .Select(x => new
                {
                    x.Name,
                    ScopeName = x.Scope?.Name
                }).ToArray();

            var claimsResTarget = result.Item2.ApiResources.Where(x => x.UserClaims != null).SelectMany(x => x.UserClaims)
                .Select(x => new
                {
                    x.Type,
                    x.ApiResourceId,
                    ApiResourceName = x.ApiResource?.Name
                }).ToArray();


            Assert.Equal(claimsIdSource.Length, claimsIdTarget.Length);
            Assert.Equal(claimsResSource.Length, claimsResTarget.Length);

        }

        [Fact]
        public void Read_Client_v3_Map_And_Copy_Client_v4_ToClean_Ids4_Database_ProofTest()
        {
            // Arrange
            var tool = new DataMigrationTool(Configuration);
            var clientsConfig = new ClientsConfigData();

            var enableScopeToApiResource2ndLevelMapping = true;// otherwise client could not claim api scopes in authorization request

            tool.CleanUpIds4Db();
            //tool.SeedClientsDataToContextIfEmpty(clientsConfig);

            //Act
            var result = tool.CopyClientTreeFromIds3to4(enableScopeToApiResource2ndLevelMapping);

            // Assert  Clients
            var clientsDataCopiedFromIds3 = tool.Ids4Tool.GetIds4ClientsRoot();

            // Assert Clients count
            Assert.Equal(result.Item2.Clients.Length, clientsDataCopiedFromIds3.Clients.Length);

            // Assert Clients by ClientId
            foreach (var client in result.Item2.Clients)
            {
                var copiedClient = clientsDataCopiedFromIds3.Clients.FirstOrDefault(x => x.ClientId == client.ClientId);
                Assert.NotNull(copiedClient);
                Assert.NotNull(copiedClient.AllowedGrantTypes);
                Assert.True(copiedClient.AllowedGrantTypes.Count > 0, $"{client.ClientName}-{client.ClientId}");
            }

            // Assert Scopes
            Assert.Equal(result.Item1.Scopes.Length, clientsDataCopiedFromIds3.ApiResources.Length + clientsDataCopiedFromIds3.IdentityResources.Length);

            // Assert Scope Claims
            var apiScopes = clientsDataCopiedFromIds3.ApiResources
                .SelectMany(x => x.Scopes).ToArray();

            // 1st level
            var apiResourceClaimsCount = clientsDataCopiedFromIds3.ApiResources.SelectMany(x => x.UserClaims).ToArray().Length;

            // 2nd level
            var apiScopeClaimsCount = apiScopes.SelectMany(y => y.UserClaims).ToArray().Length;

            var identityScopeClaimsCount = clientsDataCopiedFromIds3.IdentityResources
                .SelectMany(x => x.UserClaims).ToArray().Length;

            var claimsTotalSavedCount = (enableScopeToApiResource2ndLevelMapping ? apiScopeClaimsCount : apiResourceClaimsCount) + identityScopeClaimsCount;

            var claimsInScopesExpected = result.Item1.Scopes.SelectMany(x => x.ScopeClaims).ToArray().Length;

            Assert.Equal(claimsInScopesExpected, claimsTotalSavedCount);

            tool.SeedAdminClient(clientsConfig);

        }
    }
}
