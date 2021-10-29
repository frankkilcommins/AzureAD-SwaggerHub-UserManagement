using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SwaggerHubDemo.Services;

namespace SwaggerHubDemo
{
    public class SubscriptionManager
    {
        private readonly IConfiguration _configuration;
        private readonly IGraphService _graphService;

        public SubscriptionManager(IConfiguration configuration, IGraphService graphService)
        {
            _configuration = configuration;
            _graphService = graphService;
        }
        
        [Function("SubscriptionManager")]
        public async Task Run([TimerTrigger("0 0 12 * * *")] MyInfo myTimer, FunctionContext context)
        {
            // This function will run daily at noon 

            var logger = context.GetLogger("SubscriptionManager");
            logger.LogInformation($"SubscriptionManager function executed at: {DateTime.Now}");

            try
            {
                // Get subscriptions
                var longestToLiveSubscription = await _graphService.GetSubscriptionWithLongestTimeToLive();

                // if no subscription then create subscription
                if(longestToLiveSubscription == null)
                {
                    logger.LogInformation($"No subscription found >> creating new subscription...");
                    await _graphService.CreateChangeNotificationSubscription();
                }
                else
                {
                    // if subscription will expire in less than a week renew the subscription
                    if(DateTime.UtcNow.AddDays(7) > longestToLiveSubscription.ExpirationDateTime)
                    {
                        logger.LogInformation($"Subscription {longestToLiveSubscription.Id} will expire within 7 days >> renewing subscription...");
                        await _graphService.RenewSubscription(longestToLiveSubscription);
                    }
                    else
                    {
                        logger.LogInformation($"Subscription {longestToLiveSubscription.Id} will expire at [{longestToLiveSubscription.ExpirationDateTime}] >> no need to renew at this time...");
                    }
                }               
            }
            catch(Exception ex)
            {
                logger.LogError($"Exception thrown: {ex.Message}");
            }

            logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
        }
    }

    public class MyInfo
    {
        public MyScheduleStatus ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
