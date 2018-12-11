using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;

namespace IdentityServer4.Ids3DataMigration.Mapping.Validation
{
    public static class MapperExtensions
    {
        public static List<UnmappedProperty> GetUnmappedSimpleProperties(this IMapper mapper)
        {
            return mapper.ConfigurationProvider.GetAllTypeMaps()
                .SelectMany(m => m.GetUnmappedPropertyNames()
                .Where(x =>
                {
                    var z = m.DestinationType.GetProperty(x);
                    return z != null && (z.PropertyType.IsValueType || z.PropertyType.IsArray || z.PropertyType == typeof(string));
                })
                ////.Select(n => $"{m.SourceType.Name}->{m.DestinationType.Name} : {n}")
                .Select(n => new UnmappedProperty
                {
                    DestinationTypeName = m.DestinationType.Name,
                    PropertyName = n,
                    SourceTypeName = m.SourceType.Name
                })).OrderBy(x => x.SourceTypeName).ThenBy(x => x.PropertyName).ToList();
        }

        public static string GetMessage(this List<UnmappedProperty> unmapped, Type profileType)
        {
            return $"{profileType.FullName} mapping profile:  {string.Join(System.Environment.NewLine, unmapped.Select(x => x.ToString()))}";
        }
    }
}
