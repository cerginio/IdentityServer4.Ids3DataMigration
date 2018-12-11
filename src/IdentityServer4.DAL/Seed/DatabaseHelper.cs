using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer4.DAL.Seed
{
    public class DatabaseHelper
    {
        public static int SwitchIdentityInsertState(DbContext context, string table, string state)
        {
            var command = IdentityInsertCommands(state).Single(x => x.EndsWith($"SET IDENTITY_INSERT [dbo].{table} {state};"));
            return context.Database.ExecuteSqlCommand(new RawSqlString(command));
        }

        public static int SwitchIdentityInsertState(DbContext context, string state)
        {
            /*
            Usecase - solve that problem:
            SqlException: Cannot insert explicit value for identity column in table 'ClientCorsOrigins' when IDENTITY_INSERT is set to OFF.
                Cannot insert explicit value for identity column in table 'ClientRedirectUris' when IDENTITY_INSERT is set to OFF.
                Cannot insert explicit value for identity column in table 'ClientScopes' when IDENTITY_INSERT is set to OFF.
                Cannot insert explicit value for identity column in table 'ClientSecrets' when IDENTITY_INSERT is set to OFF.
           */

            int res = 0;
            var commands = IdentityInsertCommands(state);

            // one by one commands execution
            //for (var i = 0; i < commands.Length; i++)
            //{
            //    var c = commands[i];
            //    var upated = context.Database.ExecuteSqlCommand(new RawSqlString(c));
            //    res += upated;
            //    Console.WriteLine($"{i}-[{upated}]: {c}");
            //}

            // single connection
            var rawSqlString = new RawSqlString(string.Join(Environment.NewLine, commands));
            Console.WriteLine($"Switch Idnetity state started: {Environment.NewLine}{rawSqlString.Format}{Environment.NewLine}");
            res = context.Database.ExecuteSqlCommand(sql: rawSqlString);

            return res;
        }



        private static string[] IdentityInsertCommands(string state)
        {
            var commands = new[]
            {
                //SET IDENTITY_INSERT Table1 ON | OFF

                // ApiResources=>ApiScopes=>ApiScopeClaims
                $"SET IDENTITY_INSERT [dbo].ApiSecrets {state};",
                $"SET IDENTITY_INSERT [dbo].ApiScopeClaims {state};",
                $"SET IDENTITY_INSERT [dbo].ApiScopes {state};",

                // ApiResources=>ApiClaims
                $"SET IDENTITY_INSERT [dbo].ApiClaims {state};",
                $"SET IDENTITY_INSERT [dbo].ApiResources {state};",

                // Clients=>[Children]
                $"SET IDENTITY_INSERT [dbo].ClientCorsOrigins {state};",
                $"SET IDENTITY_INSERT [dbo].ClientGrantTypes {state};",
                $"SET IDENTITY_INSERT [dbo].ClientIdPRestrictions {state};",
                $"SET IDENTITY_INSERT [dbo].ClientPostLogoutRedirectUris {state};",
                $"SET IDENTITY_INSERT [dbo].ClientRedirectUris {state};",
                $"SET IDENTITY_INSERT [dbo].ClientSecrets {state};",

                // Clients=>Scopes=>ClientClaims
                $"SET IDENTITY_INSERT [dbo].ClientProperties {state};",
                $"SET IDENTITY_INSERT [dbo].ClientClaims {state};",
                $"SET IDENTITY_INSERT [dbo].ClientScopes {state};",
                $"SET IDENTITY_INSERT [dbo].Clients {state};",

                // IdentityResources=>IdentityClaims
                $"SET IDENTITY_INSERT [dbo].IdentityClaims {state};",
                $"SET IDENTITY_INSERT [dbo].IdentityResources {state};",

            };
            return commands;
        }

    }

}



