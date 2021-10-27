using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace SwaggerHubDemo.Models.Graph
{
  public class ResourceData
  {
    // The ID of the resource.
    [JsonPropertyName("id")]
    public string Id { get; set; }

    // The OData etag property.
    [JsonPropertyName("@odata.etag")]
    public string ODataEtag { get; set; }

    // The OData ID of the resource. This is the same value as the resource property.
    [JsonPropertyName("@odata.id")]
    public string ODataId { get; set; }

    // The OData type of the resource: "#Microsoft.Graph.Message", "#Microsoft.Graph.Event", or "#Microsoft.Graph.Contact".
    [JsonPropertyName("@odata.type")]
    public string ODataType { get; set; }

    [JsonPropertyName("members@delta")]
    public Collection<MemberData> Members { get; set; }
  }

  public class MemberData
  {
    // The OData type of the resource: "#Microsoft.Graph.User"
    [JsonPropertyName("@odata.type")]
    public string ODataType { get; set; }

    // The ID of the user resource.
    [JsonPropertyName("id")]
    public string Id { get; set; }

    // The removed indicator property
    [JsonPropertyName("@removed")]
    public string Removed { get; set; }
  }

  //public class RemovedData
  //{
  //  [JsonPropertyName("reason")]
  //  public string Reason { get; set; }
  //}
}