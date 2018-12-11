using System;
using AutoMapper;

namespace IdentityServer4.Ids3DataMigration.Mapping.Profiles
{
    using Ids3 = IdentityServer3.EntityFramework.Entities;
    using Ids4 = EntityFramework.Entities;

    public class NotFullyMatchIds3ToIds4EntityProfile : Profile
    {
        public NotFullyMatchIds3ToIds4EntityProfile()
        {
            // Semi matching
            CreateMap<Ids3.Scope, Ids4.ApiScope>()
                .ForMember(x => x.ApiResourceId, x => x.UseValue(0))//!!! TODO: handle FK
                .ReverseMap();

            CreateMap<Ids3.ScopeClaim, Ids4.ApiScopeClaim>()
                .ForMember(x => x.Type, x => x.MapFrom(s => s.Name))// stub
                .ForMember(x => x.ApiScopeId, x => x.UseValue(0))//!!! TODO: handle FK

                .ReverseMap();

            CreateMap<Ids3.ScopeSecret, Ids4.ApiSecret>()
                .ForMember(x => x.ApiResourceId, x => x.UseValue(0))//!!! TODO: handle FK
                .ForMember(x => x.Created, x => x.UseValue(DateTime.UtcNow))
                .ReverseMap();

            CreateMap<Ids3.Token, Ids4.PersistedGrant>()
                .ForMember(x => x.Type, x => x.MapFrom(s => s.TokenType))
                .ForMember(x => x.Expiration, x => x.MapFrom(s => s.Expiry.DateTime))//correct ??
                .ForMember(x => x.CreationTime, x => x.MapFrom(s => s.Expiry.DateTime.AddMinutes(-3)))//correct ??
                .ForMember(x => x.Data, x => x.MapFrom(s => s.JsonCode))
                .ReverseMap();

            //'ApiResourceId' in Scope => ApiScope mapping
            //'ApiScopeId' in ScopeClaim => ApiScopeClaim mapping
            //'ApiResourceId' in ScopeSecret => ApiSecret mapping
            //'Created' in ScopeSecret => ApiSecret mapping
        }
    }
}
