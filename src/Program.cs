using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SwaggerHubDemo.Models;
using SwaggerHubDemo.Services;
using SwaggerHubDemo.Repositories;

namespace src
{
    public class Program
    {
        public static void Main()
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

            host.Run();
        }
    }
}