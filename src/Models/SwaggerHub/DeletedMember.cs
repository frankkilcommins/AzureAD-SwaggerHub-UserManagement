using System.Text.Json.Serialization;

namespace SwaggerHubDemo.Models.SwaggerHub
{
    public class DeletedMember
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}