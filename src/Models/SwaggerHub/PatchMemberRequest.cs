using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace SwaggerHubDemo.Models.SwaggerHub
{
    public class PatchMemberRequest
    {
        [JsonPropertyName("members")]
        public Collection<ModifiedMember> Members { get; set; }
    }

    public class ModifiedMember
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }
    }
}