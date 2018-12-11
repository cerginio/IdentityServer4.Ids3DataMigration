using AutoMapper;

namespace IdentityServer4.Ids3DataMigration.Mapping.Profiles
{
    using Ids3 = IdentityServer3.EntityFramework.Entities;
    using Ids4 = EntityFramework.Entities;

    public class NotMatchIds4ToIds3EntityProfile : Profile
    {
        public NotMatchIds4ToIds3EntityProfile()
        {
            // NOT MATCH
            //CreateMap<Ids3.Consent, Ids4.Consent>().ReverseMap();

            CreateMap<Ids4.ApiResource, Ids3.ClientScope>()
                .ForMember(x => x.Scope, x => x.MapFrom(s => s.Name))
                .ReverseMap();
          

            CreateMap<Ids4.IdentityResource, Ids3.ClientScope>()
                .ForMember(x => x.Scope, x => x.MapFrom(s => s.Name))// TRASH!
                .ReverseMap();

            //  ALARM! Very different business entities
            CreateMap<Ids4.ClientProperty, Ids3.ClientClaim>()
                .ForMember(x => x.Type, x => x.MapFrom(s => s.Key))// TRASH!
                .ReverseMap();

            // ALARM! Claims not match!
            CreateMap<Ids4.IdentityClaim, Ids3.ClientClaim>()
                .ForMember(x => x.Value, x => x.MapFrom(s => s.Type));// TRASH!
            //.ReverseMap();

            CreateMap<Ids4.ApiResourceClaim, Ids3.ClientClaim>()
                .ForMember(x => x.Value, x => x.MapFrom(s => s.Type));// TRASH!
            //.ReverseMap();


            // abstarct base classes - no need to handle
            //CreateMap<Ids4.Secret, Ids3.ScopeSecret>();
            //CreateMap<Ids4.UserClaim, Ids3.ClientClaim>();

        }
    }
}