using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace SwaggerHubDemo.Models.SwaggerHub
{
    public class NewMemberResponse
    {
        [JsonPropertyName("invited")]
        public Collection<NewMember> Invited { get; set; }
    }

    public class NewMember
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}