using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace SwaggerHubDemo.Models.SwaggerHub
{
    public class MembersResponse
    {
        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("items")]
        public Collection<MemberDetail> Items { get; set; }
    }
}