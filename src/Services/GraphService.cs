using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace SwaggerHubDemo.Services
{
    public static class GraphServiceExtensions
    {
        public static void AddGraphService(this IServiceCollection services)
        {
            services.AddScoped<IGraphService, GraphService>();
        }     
    }

    public class GraphService : IGraphService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GraphService> _logger;

        public GraphService(IConfiguration configuration, ILogger<GraphService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        
        public async Task CreateChangeNotificationSubscription()
        {
            var graphClient = GetGraphServiceClient();

            var subscription = new Subscription
            {
                ChangeType = Environment.GetEnvironmentVariable("NotificationSubscriptionChangeType"),
                NotificationUrl = Environment.GetEnvironmentVariable("NotificationFunctionEndpoint"), // possibly extend with apiKey
                Resource = Environment.GetEnvironmentVariable("NotificationSubscriptionResource"),
                ExpirationDateTime = DateTime.UtcNow.AddDays(Convert.ToInt32(Environment.GetEnvironmentVariable("SubscriptionLifeTimeInDays"))),
                ClientState = Environment.GetEnvironmentVariable("NotificationClientState")
            };

            var newSubscription = await graphClient.Subscriptions.Request().AddAsync(subscription);

            _logger.LogInformation($"Subscribed. Id: {newSubscription.Id}, Expiration: {newSubscription.ExpirationDateTime}");
        }

        public async Task RenewSubscription(Subscription subscription)
        {            
            var graphClient = GetGraphServiceClient();

            var newSubscription = new Subscription 
            { 
                ExpirationDateTime = DateTime.UtcNow.AddDays(Convert.ToInt32(Environment.GetEnvironmentVariable("SubscriptionLifeTimeInDays"))) 
            };

            await graphClient.Subscriptions[subscription.Id]
                .Request()
                .UpdateAsync(newSubscription);

            _logger.LogInformation($"Renewed subscription: {subscription.Id}, New Expiration {newSubscription.ExpirationDateTime}");
        }

        public async Task<Subscription> GetSubscriptionWithLongestTimeToLive()
        {
            var graphClient = GetGraphServiceClient();
            var subscriptions = await graphClient.Subscriptions.Request().GetAsync();

            return subscriptions.Count > 0 ? subscriptions.OrderByDescending(s => s.ExpirationDateTime).FirstOrDefault() : null;
        }

        public GraphServiceClient GetGraphServiceClient()
        {
            var client = new GraphServiceClient(new DelegateAuthenticationProvider( (requestMessage) => 
            {
                var accessToken = GetAccessToken().Result;
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                return Task.FromResult(0);
            }));

            return client;            
        }

        private async Task<string> GetAccessToken()
        {
        IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(Environment.GetEnvironmentVariable("AppId"))
            .WithClientSecret(Environment.GetEnvironmentVariable("AppSecret"))
            .WithAuthority($"https://login.microsoftonline.com/{Environment.GetEnvironmentVariable("TenantId")}")
            .WithRedirectUri(Environment.GetEnvironmentVariable("AppRedirectUrl"))
            .Build();

        string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

        var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

        return result.AccessToken;
        }
    }
}