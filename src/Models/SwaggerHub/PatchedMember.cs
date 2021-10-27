using System.Text.Json.Serialization;

namespace SwaggerHubDemo.Models.SwaggerHub
{
    public class PatchedMember
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}