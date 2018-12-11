using AutoMapper;

namespace IdentityServer4.Ids3DataMigration.Mapping.Validation
{
    public static class MapperFactory
    {
        public static IMapper CreateMapper<T>() where T : Profile, new()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<T>();
            });

            /// AssertConfigurationIsValid will detect 
            /// all unmapped properties including f.e Navidation properties
            //config.AssertConfigurationIsValid();

            config.CompileMappings();
            return config.CreateMapper();
        }

        public static IMapper CreateMapper<T1,T2>() 
            where T1 : Profile, new() 
            where T2 : Profile, new()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<T1>();
                cfg.AddProfile<T2>();
            });

            /// AssertConfigurationIsValid will detect 
            /// all unmapped properties including f.e Navidation properties
            //config.AssertConfigurationIsValid();

            config.CompileMappings();
            return config.CreateMapper();
        }
    }
}
