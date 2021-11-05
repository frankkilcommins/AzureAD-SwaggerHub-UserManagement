using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SwaggerHubDemo.Services;
using SwaggerHubDemo.Repositories;
using System.IO;
using NJsonSchema;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace src
{
    public class Program
    {
        public static async Task Main()
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(configurationBuilder =>
                    configurationBuilder
                        // below is for local development
                        .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                        // below is loading the group configuration (links Azure Ad setup to SwaggerHub. You could move this to dynamic storage to be retrieve via API)
                        .AddJsonFile("GroupConfiguration.json", optional: false)
                        // below is what you need to read Application Settings in Azure
                        .AddEnvironmentVariables()
                )            
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddHttpClient();

                    //Add Custom Services
                    services.AddSwaggerHubUserManagementService();
                    services.AddGraphService();
                    
                    //Add Customer Repositories
                    services.AddSwaggerHubRepository();
                })
                .Build();

            await ValidateGroupConfiguration();

            host.Run();
        }

        private static async Task ValidateGroupConfiguration()
        {
            var text = await File.ReadAllTextAsync("GroupConfiguration.json");
            var json = JToken.Parse(text);

            var schema = await JsonSchema.FromFileAsync("GroupConfiguration.schema.json");
            var errors = schema.Validate(json);

            if(errors.Any())
            {
                var msg = $"‣ {errors.Count} total errors\n" +
                string.Join("", errors
                    .Select(e => $"  ‣ {e}[/] at " +
                                $"{e.LineNumber}:{e.LinePosition}[/]\n"));
                
                throw new InvalidDataException($"GroupConfiguration.json does not conform to it's schema. Errors: {msg}");
            }           
        }
    }
}