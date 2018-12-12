using System;
using System.Linq;
using IdentityServer4.DAL.Seed.Development;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.DAL.Seed
{
    public class DataSeeder
    {

        public static int CleanUpConfigurationDb(DbContext context)
        {
            int res = 0;
            var commands = CleanupConfigurationCommands;

            Console.WriteLine($"Db clean up started: {Environment.NewLine}");

            // Option#1: execute commands one by one
            for (var i = 0; i < commands.Length; i++)
            {
                var c = commands[i];
                var deleted = context.Database.ExecuteSqlCommand(new RawSqlString(c));
                res += deleted;
                Console.WriteLine($"{i}-[{deleted}]: {c}");
            }

            // Option#2: single connection
            //var rawSqlString = new RawSqlString(string.Join(Environment.NewLine, commands));
            //Console.WriteLine($"Db clean up started: {Environment.NewLine}{rawSqlString.Format}{Environment.NewLine}");
            //res = context.Database.ExecuteSqlCommand(rawSqlString);

            Console.WriteLine($"Db clean up complete. Deleted {res} rows");

            return res;
        }


        private static string[] CleanupConfigurationCommands
        {
            get
            {
                var commands = new[]
                {
                    // ApiResources=>ApiScopes=>ApiScopeClaims
                    "DELETE FROM [dbo].ApiSecrets where Id >0;",
                    "DELETE FROM [dbo].ApiScopeClaims where Id >0;",
                    "DELETE FROM [dbo].ApiScopes where Id >0;",

                    // ApiResources=>ApiClaims
                    "DELETE FROM [dbo].ApiClaims where Id >0;",
                    "DELETE FROM [dbo].ApiResources where Id >0;",

                    // Clients=>[Children]
                    "DELETE FROM [dbo].ClientCorsOrigins where Id >0;",
                    "DELETE FROM [dbo].ClientGrantTypes where Id >0;",
                    "DELETE FROM [dbo].ClientIdPRestrictions where Id >0;",
                    "DELETE FROM [dbo].ClientPostLogoutRedirectUris where Id >0;",
                    "DELETE FROM [dbo].ClientRedirectUris where Id >0;",
                    "DELETE FROM [dbo].ClientSecrets where Id >0;",

                    // Clients=>Scopes=>ClientClaims
                    "DELETE FROM [dbo].ClientProperties where Id >0;",
                    "DELETE FROM [dbo].ClientClaims where Id >0;",
                    "DELETE FROM [dbo].ClientScopes where Id >0;",
                    "DELETE FROM [dbo].Clients where Id >0;",

                    // IdentityResources=>IdentityClaims
                    "DELETE FROM [dbo].IdentityClaims where Id >0;",
                    "DELETE FROM [dbo].IdentityResources where Id >0;",
                };
                return commands;
            }
        }

    }
}

