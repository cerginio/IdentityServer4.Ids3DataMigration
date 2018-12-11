using Microsoft.Extensions.Configuration;

namespace IdentityServer4.DAL.Setup
{
    /// <summary>
    /// Use that mocking builder FOR TESTS ONLY to get App Settings as ConfigurationRoot
    /// </summary>
    public static class AppSettingsMockingBuilder
    {
        public static IConfigurationRoot BuildConfiguration(string environmentName)
        {
            return new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json", true, true)
                   .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                   .Build();
        }
      

    }
}
