using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Ids4Const = IdentityServer4;

namespace IdentityServer4.Ids3DataMigration.Mapping.Profiles
{

    using Ids3 = IdentityServer3.EntityFramework.Entities;
    using Ids4 = EntityFramework.Entities;

    public class Ids3ToIds4EntityProfile : Profile
    {
        private int id = 1000;

        public Ids3ToIds4EntityProfile()
        {
            CreateMap<Ids3.Client, Ids4.Client>()
                // http://confluence.ifint.biz/display/IdM2/Client+configuration+gap+between+Ids3+and+Ids4
                // Valid: 
                .ForMember(x => x.ProtocolType, x => x.UseValue(Ids4Const.IdentityServerConstants.ProtocolTypes.OpenIdConnect))
                .ForMember(x => x.ConsentLifetime, x => x.UseValue(default(int?)))
                .ForMember(x => x.FrontChannelLogoutSessionRequired, x => x.MapFrom(t => t.LogoutSessionRequired))
                .ForMember(x => x.FrontChannelLogoutUri, x => x.MapFrom(t => t.LogoutUri))
                .ForMember(x => x.ClientClaimsPrefix, x => x.MapFrom(t => t.PrefixClientClaims))
                .ForMember(x => x.Description, x => x.MapFrom(s => s.ClientName))

                .ForMember(x => x.Created, x => x.UseValue(DateTime.UtcNow))
                .ForMember(x => x.LastAccessed, x => x.UseValue(default(DateTime?)))
                .ForMember(x => x.Updated, x => x.UseValue(default(DateTime?)))

                .ForMember(x => x.DeviceCodeLifetime, x => x.UseValue(default(int)))
                .ForMember(x => x.UserSsoLifetime, x => x.UseValue(default(int?)))
                .ForMember(x => x.NonEditable, x => x.UseValue(default(bool)))
                .ForMember(x => x.UserCodeType, x => x.UseValue(default(string)))

                // Collections
                // Skip all collections that match by name

                // AllowedCustomGrantTypes conditional mapping:
                //.ForMember(x => x.AllowedGrantTypes, x => x.MapFrom(t => t.AllowedCustomGrantTypes))
                .ForMember(x => x.AllowedGrantTypes, x => x.ResolveUsing(client =>
                {
                    if (client.Flow == IdentityServer3.Core.Models.Flows.Custom)
                    {
                        return client.AllowedCustomGrantTypes;
                    }

                    // Get GrantType by Flow
                    string grantType = client.Flow.ToString();

                    switch (client.Flow)
                    {
                        case IdentityServer3.Core.Models.Flows.AuthorizationCode:
                            grantType = Ids4Const.Models.GrantType.AuthorizationCode;
                            break;

                        case IdentityServer3.Core.Models.Flows.Implicit:
                            grantType = Ids4Const.Models.GrantType.Implicit;
                            break;

                        case IdentityServer3.Core.Models.Flows.Hybrid:
                            grantType = Ids4Const.Models.GrantType.Hybrid;
                            break;

                        case IdentityServer3.Core.Models.Flows.ClientCredentials:
                            grantType = Ids4Const.Models.GrantType.ClientCredentials;
                            break;

                        case IdentityServer3.Core.Models.Flows.ResourceOwner:
                            grantType = Ids4Const.Models.GrantType.ResourceOwnerPassword;
                            break;


                        // default: client.Flow
                        case IdentityServer3.Core.Models.Flows.Custom:
                        case IdentityServer3.Core.Models.Flows.AuthorizationCodeWithProofKey:
                        case IdentityServer3.Core.Models.Flows.HybridWithProofKey:

                        default:
                            break;
                    }

                    id++;

                    return new List<Ids3.ClientCustomGrantType>()
                    {
                        new Ids3.ClientCustomGrantType()
                        {
                            GrantType = grantType,
                            Client = client,
                            Id = id
                        }
                    };

                }))

                // scafolding:
                //.ForMember(x => x.YYYY, x => x.Ignore())
                //.ForMember(x => x.YYYY, x => x.MapFrom(s.YYYY))
                //.ForMember(x => x.YYYY, x => x.UseValue(default(string)))
                // Ignore is the worst option

                // Described remapping recommendations in confluence per each Client setting heere:
                // http://confluence.ifint.biz/display/IdM2/Client+configuration+gap+between+Ids3+and+Ids4
                // TODO: think about gap and provide correct default values
                .ForMember(x => x.AllowOfflineAccess, x => x.UseValue(false))
                .ForMember(x => x.AllowPlainTextPkce, x => x.UseValue(false))
                .ForMember(x => x.AlwaysIncludeUserClaimsInIdToken, x => x.UseValue(true))
                .ForMember(x => x.BackChannelLogoutSessionRequired, x => x.UseValue(false))
                .ForMember(x => x.BackChannelLogoutUri, x => x.UseValue(default(string)))
                .ForMember(x => x.PairWiseSubjectSalt, x => x.UseValue("undefined"))
                .ForMember(x => x.RequireClientSecret, x => x.UseValue(true))
                .ForMember(x => x.RequirePkce, x => x.UseValue(true))
                .ForMember(x => x.UpdateAccessTokenClaimsOnRefresh, x => x.UseValue(true));
            // Some reason ReverseMap not pass validation test 
            //.ForMember(x => x.ClientName, x => x.MapFrom(s => s.ClientName))
            //.ForSourceMember(x => x.ClientName, x => x.Ignore())
            //.ReverseMap();


            CreateMap<Ids3.ClientCorsOrigin, Ids4.ClientCorsOrigin>().ReverseMap();
            CreateMap<Ids3.ClientCustomGrantType, Ids4.ClientGrantType>().ReverseMap();
            CreateMap<Ids3.ClientIdPRestriction, Ids4.ClientIdPRestriction>().ReverseMap();

            CreateMap<Ids3.ClientPostLogoutRedirectUri, Ids4.ClientPostLogoutRedirectUri>()
                .ForMember(x => x.PostLogoutRedirectUri, x => x.MapFrom(s => s.Uri))
                .ReverseMap();

            CreateMap<Ids3.ClientRedirectUri, Ids4.ClientRedirectUri>()
                .ForMember(x => x.RedirectUri, x => x.MapFrom(s => s.Uri))
                .ReverseMap();

            // ** Secrets **
            CreateMap<Ids3.ClientSecret, Ids4.ClientSecret>()
                // Id, Description, Value, Expiration, Type => OK
                .ForMember(x => x.Created, x => x.UseValue(DateTime.UtcNow))
                .ForMember(x => x.Expiration, x => x.MapFrom(s => s.Expiration))// DateTime? => DateTimeOffset
                .ReverseMap();


            CreateMap<Ids3.ScopeSecret, Ids4.ApiSecret>()
                // Id, Description, Value, Expiration, Type => OK
                .ForMember(x => x.Created, x => x.UseValue(DateTime.UtcNow))
                .ForMember(x => x.ApiResourceId, x => x.MapFrom(y => y.Scope.Id))
                //!!! TODO: handle FK
                .ForMember(x => x.Expiration, x => x.MapFrom(s => s.Expiration))// DateTime? => DateTimeOffset
                .ReverseMap();



            // ClientClaim => 0 items on ATest
            CreateMap<Ids3.ClientClaim, Ids4.ClientClaim>()
                // Id, Type, Value => OK
                .ReverseMap();


            // *** Scopes ***
            //  Level #0
            CreateMap<Ids3.ClientScope, Ids4.ClientScope>()
                // Id, Scope => OK
                .ReverseMap();

            // Level #1 Scope {Type = Identity}  => IdentityResource 
            CreateMap<Ids3.Scope, Ids4.IdentityResource>()
                // Enabled, Required, Emphasize, Name, DisplayName, Description, ShowInDiscoveryDocument => OK
                // ScopeClaim => IdentityClaim 
                // ScopeSecrets => NOK
                .ForMember(x => x.Created, x => x.UseValue(DateTime.UtcNow))
                .ForMember(x => x.Updated, x => x.UseValue(default(DateTime?)))
                .ForMember(x => x.NonEditable, x => x.UseValue(default(bool)))

                //.ForMember(x => x.UserClaims, x => x.MapFrom(y => y.ScopeClaims))// reworked to =>
                .ForMember(x => x.UserClaims, x => x.ResolveUsing(scope =>
                {
                    return scope.ScopeClaims.Where(sc => sc.Scope.Type == 0).ToList();//ScopeType.Identity = 0
                }));


            // Level #1 Scope {Type = Resource}  => ApiResource 
            CreateMap<Ids3.Scope, Ids4.ApiResource>()
                //Id,  Enabled,  Name, DisplayName, Description => OK
                // Collections
                .ForMember(x => x.Scopes, x => x.UseValue(new List<Ids4.ApiScope>()))// recursive Level #1 => (Level #1 (ApiResources..ApiClaims), Level #2 (ApiScopes..ApiScopeClaims))
                                                                                     // 2 possible scenarios: Level #1 OR Level #1 | Level #2
                .ForMember(x => x.Created, x => x.UseValue(DateTime.UtcNow))
                .ForMember(x => x.Updated, x => x.UseValue(default(DateTime?)))
                .ForMember(x => x.LastAccessed, x => x.UseValue(default(DateTime?)))
                .ForMember(x => x.Secrets, x => x.MapFrom(y => y.ScopeSecrets))
                .ForMember(x => x.NonEditable, x => x.UseValue(default(bool)))

                // ScopeClaim => ApiResourceClaim 
                //.ForMember(x => x.UserClaims, x => x.MapFrom(y => y.ScopeClaims))// reworked to =>
                .ForMember(x => x.UserClaims, x => x.ResolveUsing(scope =>
                {
                    return scope.ScopeClaims.Where(sc => sc.Scope.Type == 1).ToList();//ScopeType.Resource = 1
                }));


            // ScopeClaim
            CreateMap<Ids3.ScopeClaim, Ids4.IdentityClaim>()
                // Name, Description, AlwaysIncludeInIdToken => NOK
                // Id, Type => OK. Target => IdentityClaim: UserClaim
                .ForMember(x => x.Type, x => x.MapFrom(s => s.Name))
                .ForMember(x => x.IdentityResourceId, x => x.MapFrom(s => s.Scope == null ? -1 : s.Scope.Id))
                //!!! TODO: handle FK
                // TODO: clarify Value?
                //.ForMember(x => x.??, x => x.MapFrom(s => s.Value))
                .ReverseMap();


            CreateMap<Ids3.ScopeClaim, Ids4.ApiResourceClaim>()
                // Name, Description, AlwaysIncludeInIdToken => NOK
                // Id, Type => OK. Target => ApiResourceClaim: UserClaim
                // TODO: clarify Type ???
                .ForMember(x => x.Type, x => x.MapFrom(s => s.Name))
                .ForMember(x => x.ApiResourceId, x => x.MapFrom(s => s.Scope == null ? -1 : s.Scope.Id))
                //!!! TODO: handle FK
                .ReverseMap();





            // NOT IN USE, STUB for tests!
            // SUCH Mapping COULD NOT BE CORRECT, NOT REACHABLE on Automapper level, because ApiResource chlid entity not yet created in Ids4.Db
            //TODO: check requirements if there is a case to map FROM first TO first and second level?
            // Level #2, related to Scope {Type = Resource}  => ApiScope  
            CreateMap<Ids3.Scope, Ids4.ApiScope>()
                // Id, Required, Emphasize, Name, DisplayName, Description, ShowInDiscoveryDocument => OK
                // ScopeSecrets => NOK
                .ForMember(x => x.UserClaims, x => x.MapFrom(y => y.ScopeClaims))// ApiScope..ApiScopeClaim => Scope{Type = Resource}..ScopeClaim
                .ForMember(x => x.ApiResourceId, x => x.MapFrom(y => y.Id));// Not correct reference
            //!!! TODO: handle FK
            //.ReverseMap();

            // Level #2, Scope {Type = Resource}..ScopeClaim => ApiScope..ApiScopeClaim
            CreateMap<Ids3.ScopeClaim, Ids4.ApiScopeClaim>()
                // Name, Description, AlwaysIncludeInIdToken => NOK
                // Id, Type => OK.  Target => ApiScopeClaim: UserClaim
                // TODO: clarify Type ???
                .ForMember(x => x.Type, x => x.MapFrom(s => s.Name))
                .ForMember(x => x.ApiScopeId, x => x.MapFrom(s => s.Scope == null ? -1 : s.Scope.Id))
                //!!! TODO: handle FK
                .ReverseMap();
        }
    }


    public class Ids3ToIds4ClientOnlyEntityProfile : Profile
    {
        public Ids3ToIds4ClientOnlyEntityProfile()
        {

            CreateMap<Ids3.Client, Ids4.Client>()

                //.ForMember(x => x.YYYY, x => x.Ignore())
                // Ignore is the worst option
                // TODO: think about gap and provide correct default values
                // Describe remapping recommendations in confluence per each Client setting

                .ForMember(x => x.AllowOfflineAccess, x => x.UseValue(false))
                .ForMember(x => x.AllowPlainTextPkce, x => x.UseValue(false))
                .ForMember(x => x.AlwaysIncludeUserClaimsInIdToken, x => x.UseValue(true))
                .ForMember(x => x.BackChannelLogoutSessionRequired, x => x.UseValue(true))
                .ForMember(x => x.BackChannelLogoutUri, x => x.UseValue("/undefined"))
                .ForMember(x => x.ClientClaimsPrefix, x => x.UseValue(""))
                .ForMember(x => x.ConsentLifetime, x => x.UseValue(180))
                .ForMember(x => x.Description, x => x.MapFrom(s => s.ClientName))
                .ForMember(x => x.FrontChannelLogoutSessionRequired, x => x.UseValue(true))
                .ForMember(x => x.FrontChannelLogoutUri, x => x.UseValue("/undefined"))
                .ForMember(x => x.PairWiseSubjectSalt, x => x.UseValue("undefined"))
                .ForMember(x => x.ProtocolType, x => x.UseValue("https"))
                .ForMember(x => x.RequireClientSecret, x => x.UseValue(true))
                .ForMember(x => x.RequirePkce, x => x.UseValue(true))
                .ForMember(x => x.UpdateAccessTokenClaimsOnRefresh, x => x.UseValue(true));
            // Some reason ReverseMap not pass validation test 
            //.ForMember(x => x.ClientName, x => x.MapFrom(s => s.ClientName))
            //.ForSourceMember(x => x.ClientName, x => x.Ignore())
            //.ReverseMap();
        }
    }

    public enum Flows
    {
        AuthorizationCode,
        Implicit,
        Hybrid,
        ClientCredentials,
        ResourceOwner,
        Custom,
        AuthorizationCodeWithProofKey,
        HybridWithProofKey,
    }
}