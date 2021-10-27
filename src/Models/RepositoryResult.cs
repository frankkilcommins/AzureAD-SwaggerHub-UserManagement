using System.Net;

namespace SwaggerHubDemo.Models
{
    public class RepositoryResult<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccessCode { get; set; }
        public T Data { get; set; }
        public ErrorModel Error { get; set; }
    }
}