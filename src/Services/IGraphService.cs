using System.Threading.Tasks;
using Microsoft.Graph;

namespace SwaggerHubDemo.Services
{
    public interface IGraphService
    {
        GraphServiceClient GetGraphServiceClient();
        Task<Subscription> GetSubscriptionWithLongestTimeToLive();
        Task RenewSubscription(Subscription subscription);
        Task CreateChangeNotificationSubscription();
    }
}