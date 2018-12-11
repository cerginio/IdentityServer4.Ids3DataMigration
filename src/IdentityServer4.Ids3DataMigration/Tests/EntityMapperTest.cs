using AutoMapper;
using IdentityServer4.Ids3DataMigration.Mapping.Profiles;
using IdentityServer4.Ids3DataMigration.Mapping.Validation;
using Xunit;

namespace IdentityServer4.Ids3DataMigration.Tests
{
    // Detecting DTO and Entities missing or misconfigured properties
    // https://stackoverflow.com/questions/41823894/detecting-dto-and-entities-missing-or-misconfigured-properties/41824521#41824521
    [Trait("Category", "MappingsTest")]
    [Trait("Category", "DAL")]
    public class EntityMapperTest
    {
        [Fact]
        public void Ids3ToIds4Entity_Mapping_Profiles_Must_Not_Have_Unmapped_Properties()
        {
            TestProfile<Ids3ToIds4EntityProfile>(0);

            TestProfile<NotFullyMatchIds3ToIds4EntityProfile>(0);

            // TRASH test
            TestProfile<NotMatchIds4ToIds3EntityProfile>(0);
        }

        private static void TestProfile<T>(int expectedCount = 0) where T : Profile, new()
        {
            var mapper = MapperFactory.CreateMapper<T>();
            var unmappedProperties = mapper.GetUnmappedSimpleProperties();
            Assert.True(unmappedProperties.Count == expectedCount, unmappedProperties.GetMessage(typeof(T)));
        }


        [Fact]
        public void Test_Mapping_Profile_Must_Detect_Unmapped_Properties()
        {
            TestProfile<TestMappingProfile>(12);
        }
    }
}