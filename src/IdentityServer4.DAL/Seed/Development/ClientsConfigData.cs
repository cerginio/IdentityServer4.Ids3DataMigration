// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Test;
using ApiResource = IdentityServer4.Models.ApiResource;
using Client = IdentityServer4.Models.Client;
using IdentityResource = IdentityServer4.Models.IdentityResource;
using Secret = IdentityServer4.Models.Secret;

namespace IdentityServer4.DAL.Seed.Development
{
    public class ClientsConfigData : IClientsConfigData
    {
        // clients want to access resources (aka scopes)
        public IEnumerable<Client> GetClients()
        {
            // client credentials client
            return new List<Client>
            {

                new Client
                {
                    ClientName = "Test Client",
                    ClientId = "testClient",
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequireConsent = false,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris =
                    {
                        "https://localhost:44390/",
                        "https://localhost:44390/index.html",
                        "https://localhost:44390/popup.html",
                        "https://localhost:44390/renew.html",
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,

                        "testApi"
                    },
                    AllowedCorsOrigins = {"https://localhost:44390"}
                },

                new Client
                {
                    ClientName = "testMvc",
                    ClientId = "testMvc",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequireConsent = false,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris =
                    {
                        "http://localhost:1391/signin-oidc",
                    },
                    PostLogoutRedirectUris =
                    {
                        "http://localhost:1391/signout-callback-oidc",
                        "http://localhost:1391/Account/SignedOut",
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "testApi"
                    }
                },

                new Client
                {
                    ClientName = "JavaScript Implicit Client",
                    ClientId = "testJsImplicit",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequireConsent = false,
                    AllowAccessTokensViaBrowser = true,

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "testApi"
                    },

                    RedirectUris =
                    {
                        "http://localhost:21575/index.html",
                        "http://localhost:21575/silent_renew.html",
                        "http://localhost:21575/popup_callback.html",
                    },

                    PostLogoutRedirectUris = {"http://localhost:21575/index.html",},
                    AllowedCorsOrigins = {"http://localhost:21575",},

                    AccessTokenLifetime = 3600,
                    AccessTokenType = AccessTokenType.Jwt
                },

                    ///////////////////////////////////////////
                    // Skoruba.IdentityServer4.Admin Client
                    //////////////////////////////////////////
                    new Client
                    {
                        ClientId = AdminPortalConsts.OidcClientId,
                        ClientName = AdminPortalConsts.OidcClientId,

                        AllowedGrantTypes = GrantTypes.Implicit,
                        RequireConsent = false,
                        AllowAccessTokensViaBrowser = true,

                        RedirectUris = {
                            $"{AdminPortalConsts.IdentityAdminBaseUrl1}/signin-oidc",
                            $"{AdminPortalConsts.IdentityAdminBaseUrl2}/signin-oidc"},
                        //FrontChannelLogoutUri = $"{AdminPortalConsts.IdentityAdminBaseUrl1}/signout-oidc",
                        PostLogoutRedirectUris = {
                            $"{AdminPortalConsts.IdentityAdminBaseUrl1}/signout-callback-oidc",
                            $"{AdminPortalConsts.IdentityAdminBaseUrl2}/signout-callback-oidc" },

                        AllowedCorsOrigins =
                        {
                            AdminPortalConsts.IdentityAdminBaseUrl1, AdminPortalConsts.IdentityAdminBaseUrl2
                        },

                        AllowedScopes =
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            IdentityServerConstants.StandardScopes.Email,
                            "roles",
                        }
                    },
            };
        }

        public List<TestUser> GetTestUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "alice",
                    Password = "alice",

                    Claims = new List<Claim>
                    {
                        new Claim("name", "Alice"),
                        new Claim("website", "https://alice.com"),
                        new Claim(JwtClaimTypes.Role, AdminPortalConsts.AdministrationRole),

                    }
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "bob",
                    Password = "bob",

                    Claims = new List<Claim>
                    {
                        new Claim("name", "Bob"),
                        new Claim("website", "https://bob.com"),
                        new Claim(JwtClaimTypes.Role, AdminPortalConsts.AdministrationRole),

                    }
                }
            };
        }




        // scopes define the resources in your system
        public IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
            };
        }

        public IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("api1", "My API")
            };
        }

    }


        public class AdminPortalConsts
        {
            public const string AdministrationRole = "Idm_Administrator";
            public const string IdentityAdminBaseUrl1 = "http://adminalfa.azurewebsites.net";
            public const string IdentityAdminBaseUrl2 = "http://localhost:5000";
            public const string OidcClientId = "identity_admin";
        }
}