using System.Linq;
using IdentityServer4.DAL.Seed.Development;
using IdentityServer4.DAL.Setup;
using IdentityServer4.Ids3DataMigration.Tools;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IdentityServer4.Ids3DataMigration.Scenarios
{

    [Trait("Category", "DAL")]
    public class DataMigrationScenarios
    {
        //private const string ScenarioName = "Alfa-to-Beta-scenario";

        //private static readonly IConfigurationRoot Configuration =
        //    new ConfigurationBuilder()
        //    .AddJsonFile("appsettings.json", true, true)
        //    .AddJsonFile($"appsettings.{ScenarioName}.json", true, true)
        //.Build();

        //private IConfigurationRoot Configuration => AppSettingsMockingBuilder.BuildConfiguration("alfa-to-local-scenario");
        // alfa-to-local-scenario | local-to-local-scenario | alfa-to-beta-scenario

        private IConfigurationRoot Configuration => AppSettingsMockingBuilder.BuildConfiguration();

        [Fact]
        public void MigrateIds3ClientsToRestoredIds4Database()
        {
            // Arrange
            var tool = new DataMigrationTool(Configuration);
            var clientsConfig = new ClientsConfigData();
            var enableScopeToApiResource2ndLevelMapping = true;

            tool.CleanUpIds4Db();
            //tool.SeedClientsDataToContextIfEmpty(clientsConfig);

            //Act
            var result = tool.CopyClientsScopesTreeFromIds3DbToIds4Db(enableScopeToApiResource2ndLevelMapping);

            // Assert  Clients
            var clientsDataCopiedFromIds3 = tool.Ids4Tool.GetIds4ClientsRoot();

            RoughlyCheckCopyResults(result, clientsDataCopiedFromIds3);

            tool.SeedAdminClient(clientsConfig);
        }



        private static void RoughlyCheckCopyResults((Ids3RootDTO, Ids4RootDTO) result, Ids4RootDTO clientsDataCopiedFromIds3)
        {
            // Assert Clients count
            Assert.True(result.Item2.Clients.Length <= clientsDataCopiedFromIds3.Clients.Length);

            // Assert Clients by ClientId
            foreach (var client in result.Item2.Clients)
            {
                Assert.NotNull(clientsDataCopiedFromIds3.Clients.FirstOrDefault(x => x.ClientId == client.ClientId));
            }

            // Assert Scopes
            Assert.True(result.Item1.Scopes.Length <= clientsDataCopiedFromIds3.ApiResources.Length +
                        clientsDataCopiedFromIds3.IdentityResources.Length);

            // Assert Scope Claims
            var apiScopes = clientsDataCopiedFromIds3.ApiResources
                .SelectMany(x => x.Scopes).ToArray();


            var apiScopeClaimsCount = apiScopes.SelectMany(y => y.UserClaims).ToArray().Length;

            var identityScopeClaimsCount = clientsDataCopiedFromIds3.IdentityResources
                .SelectMany(x => x.UserClaims).ToArray().Length;


            var claimsTotalSavedCount = apiScopeClaimsCount + identityScopeClaimsCount;

            var claimsInScopesExpected = result.Item1.Scopes.SelectMany(x => x.ScopeClaims).ToArray().Length;

            Assert.True(claimsInScopesExpected <= claimsTotalSavedCount);
        }
    }
}
