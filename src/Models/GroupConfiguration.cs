using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace SwaggerHubDemo.Models
{

    public class GroupConfiguration
    {
        public const string Position = "GroupConfiguration";
        public Collection<ActiveDirectoryGroup> ActiveDirectoryGroups { get; set; }
    }

    public class ActiveDirectoryGroup
    {
        [JsonPropertyName("objectId")]

        public string ObjectId { get; set; }

        [JsonPropertyName("name")]

        public string Name { get; set; }

        [JsonPropertyName("swaggerHubRole")]

        public string SwaggerHubRole { get; set; }

        [JsonPropertyName("organizations")]
        public Collection<Organization> Organizations { get; set; }

    }

    public class Organization
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}