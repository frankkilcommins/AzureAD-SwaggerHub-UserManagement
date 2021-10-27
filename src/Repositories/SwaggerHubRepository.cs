using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SwaggerHubDemo.Models;

namespace SwaggerHubDemo.Repositories
{
    public static class SwaggerHubRepositoryExtensions
    {
        public static void AddSwaggerHubRepository(this IServiceCollection services)
        {
            services.AddScoped<ISwaggerHubRepository, SwaggerHubRepository>();
        }
    }

    public class SwaggerHubRepository : ISwaggerHubRepository
    {
        public async Task<RepositoryResult<T>> Get<T>(string url, string query)
        {
            return await SendSwaggerHubRequest<T>(HttpMethod.Get, url, query, null);
        }

        public async Task<RepositoryResult<T>> Post<T>(string url, object content)
        {
            return await SendSwaggerHubRequest<T>(HttpMethod.Post, url, null, content);
        }

        public async Task<RepositoryResult<T>> Patch<T>(string url, object content)
        {
            return await SendSwaggerHubRequest<T>(HttpMethod.Patch, url, null, content);
        }

        public async Task<RepositoryResult<T>> Delete<T>(string url, string query)
        {
            return await SendSwaggerHubRequest<T>(HttpMethod.Delete, url, query, null);
        }

        private SwaggerHubSettings SetSwaggerHubSettings()
        {
            return new SwaggerHubSettings()
            {
                BaseUrl = Environment.GetEnvironmentVariable("SwaggerHubBaseUrl"),
                UserManagementApiVersion = Environment.GetEnvironmentVariable("SwaggerHubUserManagementApiVersion"),
                UserManagementApiPath = Environment.GetEnvironmentVariable("SwaggerHubUserManagementApiPath"),
                ApiKey = Environment.GetEnvironmentVariable("SwaggerHubApiKey")
            };
        }
        private async Task<RepositoryResult<T>> SendSwaggerHubRequest<T>(HttpMethod httpMethod, string url, string query, object bodyContent)
        {
            try
            {
                var swaggerHubSettings = SetSwaggerHubSettings();
                var serializerSettings = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };

                using (var httpClient = new HttpClient())
                {
                    url = $"{swaggerHubSettings.BaseUrl}{swaggerHubSettings.UserManagementApiPath}/{swaggerHubSettings.UserManagementApiVersion}{url}";

                    if (!string.IsNullOrEmpty(query))
                    {
                        url += $"?{query}";
                    }

                    var request = new HttpRequestMessage(httpMethod, url);
                    request.Headers.TryAddWithoutValidation("Authorization", swaggerHubSettings.ApiKey);
                    request.Headers.TryAddWithoutValidation("Accept", "application/json");

                    if (bodyContent != null)
                    {
                        var serializedContent = JsonSerializer.Serialize(bodyContent, serializerSettings); 
                        request.Content = new StringContent(serializedContent, System.Text.Encoding.UTF8, "application/json");
                    }

                    var result = new RepositoryResult<T> { IsSuccessCode = false, StatusCode = HttpStatusCode.InternalServerError };

                    try
                    {
                        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, new CancellationToken()).ConfigureAwait(false);

                        if (response.IsSuccessStatusCode)
                        {
                            result.StatusCode = response.StatusCode;
                            result.IsSuccessCode = true;

                            if(response.Content != null)
                            {
                                string jsonResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                                result.Data = JsonSerializer.Deserialize<T>(jsonResult);
                            }                            
                        }
                        else
                        {
                            result.IsSuccessCode = false;

                            string jsonResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            var error = JsonSerializer.Deserialize<ErrorModel>(jsonResult); 

                            result.StatusCode = response.StatusCode;
                            result.Error = error;
                        }
                    }
                    catch (HttpRequestException httpException)
                    {
                        result.StatusCode = HttpStatusCode.InternalServerError;
                        result.IsSuccessCode = false;

                        result.Error = new ErrorModel { Id = "internal_server_error", Message = httpException.Message };
                    }

                    return result;
                }
            }
            catch(Exception ex)
            {
                var result = new RepositoryResult<T>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccessCode = false,

                    Error = new ErrorModel { Id = "internal_server_error", Message = ex.Message }
                };
                return result;
            }
              
        }
    }
}