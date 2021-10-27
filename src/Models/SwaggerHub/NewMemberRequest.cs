using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace SwaggerHubDemo.Models.SwaggerHub
{
    public class NewMemberRequest
    {
        [JsonPropertyName("members")]
        public Collection<Member> Members { get; set; }
    }
}