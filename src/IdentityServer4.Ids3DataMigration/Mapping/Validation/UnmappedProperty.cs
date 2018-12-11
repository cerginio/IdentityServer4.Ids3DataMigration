namespace IdentityServer4.Ids3DataMigration.Mapping.Validation
{
    public class UnmappedProperty
    {
        public string PropertyName { get; set; }
        public string DestinationTypeName { get; set; }
        public string SourceTypeName { get; set; }

        public override string ToString()
        {
            return $"'{this.PropertyName}' in {this.SourceTypeName} => {this.DestinationTypeName} mapping";
        }
    }  
}
