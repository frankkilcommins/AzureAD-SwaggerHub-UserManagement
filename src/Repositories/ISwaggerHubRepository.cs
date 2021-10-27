using System.Threading.Tasks;
using SwaggerHubDemo.Models;

namespace SwaggerHubDemo.Repositories
{
    public interface ISwaggerHubRepository
    {
        Task<RepositoryResult<T>> Get<T>(string url, string query);
        Task<RepositoryResult<T>> Post<T>(string url, object content);
        Task<RepositoryResult<T>> Patch<T>(string url, object content);
        Task<RepositoryResult<T>> Delete<T>(string url, string query);
    }
}