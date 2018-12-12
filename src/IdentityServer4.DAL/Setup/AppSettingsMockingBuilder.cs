using Microsoft.Extensions.Configuration;

namespace IdentityServer4.DAL.Setup
{
    /// <summary>
    /// Use that mocking builder FOR TESTS ONLY to get App Settings as ConfigurationRoot
    /// </summary>
    public static class AppSettingsMockingBuilder
    {
        //public static IConfigurationRoot BuildConfiguration(string environmentName)
        //{
        //    return new ConfigurationBuilder()
        //           .AddJsonFile("appsettings.json", true, true)
        //           .AddJsonFile($"appsettings.{environmentName}.json", true, true)
        //           .Build();
        //}

        //public static IConfigurationRoot BuildConfiguration()
        //{
        //    var rootCfg = new ConfigurationBuilder()
        //        .AddJsonFile("appsettings.json", true, true)
        //        .Build();

        //    var scenarioName = rootCfg["ScenarioName"];

        //    return new ConfigurationBuilder()
        //        .AddJsonFile("appsettings.json", true, true)
        //        .AddJsonFile($"appsettings.{scenarioName}.json", true, true)
        //        .Build();
        //}

        // 2 in 1
        public static IConfigurationRoot BuildConfiguration(string environmentName = null)
        {
            if (environmentName == null)
            {
                var rootCfg = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", true, true)
                    .Build();

                environmentName = rootCfg["ScenarioName"];
            }

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .Build();
        }
    }
}
