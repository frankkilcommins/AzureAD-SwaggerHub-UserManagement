using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SwaggerHubDemo.Models;
using SwaggerHubDemo.Models.Graph;
using System;
using SwaggerHubDemo.Services;

namespace SwaggerHubDemo
{
    public class AzureWebhookCallbackToManageSwaggerHubUsers
    {
        private readonly IConfiguration _configuration;
        private readonly ISwaggerHubUserManagementService _swaggerHubUserManagementService;
        private readonly IGraphService _graphService;
        public GroupConfiguration _groupConfiguration {get; private set;}
    
        public AzureWebhookCallbackToManageSwaggerHubUsers(IConfiguration configuration
            ,ISwaggerHubUserManagementService swaggerHubUserManagementService
            ,IGraphService graphService)
        {
            _configuration = configuration;
            _swaggerHubUserManagementService = swaggerHubUserManagementService;
            _graphService = graphService;
            _groupConfiguration = InitializeGroupConfiguration(); 
        }

        private GroupConfiguration InitializeGroupConfiguration()
        {
            GroupConfiguration groupConfiguration = new GroupConfiguration();
            _configuration.GetSection(GroupConfiguration.Position).Bind(groupConfiguration);
            return groupConfiguration;
        }

        [Function("AzureWebhookCallbackToManageSwaggerHubUsers")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("AzureWebhookCallbackToManageSwaggerHubUsers");          
            logger.LogInformation("AzureWebhookCallbackToManageSwaggerHubUsers received request");

            try
            {
                // basic validation of request
                var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
                string token = query["validationToken"];
                
                if(!string.IsNullOrEmpty(token))
                {
                    //acknowledge back with validation token
                    logger.LogInformation($"Validation Token: {token}");
                    var ackResponse = req.CreateResponse(HttpStatusCode.OK);
                    ackResponse.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                    ackResponse.WriteString(token);
                    return ackResponse;
                }  

                //ToDo - validate API Key        

                // handle the notifications
                using(StreamReader reader = new StreamReader(req.Body))
                {
                    string content = await reader.ReadToEndAsync();
                    logger.LogInformation($"Received Body: {content}");

                    var notifications = JsonSerializer.Deserialize<Notifications>(content);

                    foreach(var notification in notifications.Items)
                    {
                        logger.LogInformation($"Received notification: '{notification.Resource}', {notification.ResourceData?.Id}");

                        // Validate Client State
                        if(!string.Equals(notification.ClientState, Environment.GetEnvironmentVariable("NotificationClientState")))
                        {
                            logger.LogWarning($"Unknown notification clientState. Received: '{notification.ClientState}'");
                            return req.CreateResponse(HttpStatusCode.Unauthorized);
                        }

                        //Validate Groups
                        if(IsKnownGroup(notification.ResourceData?.Id) && notification.ResourceData?.Members != null)
                        {
                            var graphServiceClient = _graphService.GetGraphServiceClient();

                            // Process the users
                            foreach(var member in notification.ResourceData?.Members)
                            {
                                string changeType = member?.Removed ?? "added";
                                logger.LogInformation($"UserId: {member.Id}, ChangeType: {changeType}");

                                //Call Graph to retrieve user details from Azure AD
                                var graphUser = new Microsoft.Graph.User();
                                var user = await graphServiceClient.Users[member.Id].Request().GetAsync();

                                if(user != null)
                                {
                                    logger.LogInformation($"UserId: {member.Id}, DisplayName: {user.DisplayName}, Email: {user.Mail}"); //ToDo - verify otherMails for robust email capture

                                    //Update, Create or Delete user from SwaggerHub
                                    if(changeType == "added")
                                    {
                                        logger.LogInformation("Adding user to SwaggerHub...");

                                        // add the user to the Swagger Hub Orgs
                                        await _swaggerHubUserManagementService.CreateMember(logger
                                            ,_groupConfiguration.ActiveDirectoryGroups.FirstOrDefault(g => g.ObjectId == notification.ResourceData?.Id)
                                            ,user.GivenName
                                            ,user.Surname
                                            ,user.Mail
                                        );                                 

                                    }
                                    else if(changeType == "deleted")
                                    {                                       
                                        logger.LogInformation("Deleting user from SwaggerHub...");

                                        await _swaggerHubUserManagementService.DeleteMember(logger
                                            ,_groupConfiguration.ActiveDirectoryGroups.FirstOrDefault(g => g.ObjectId == notification.ResourceData?.Id)
                                            ,user.Mail
                                        );
                                    }
                                }
                            }
                        }                  
                    }
                }
            }
            catch(Exception ex)
            {
                logger.LogError($"Exception thrown: {ex.Message}");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }

        private bool IsKnownGroup(string resourceId)
        {    
            return _groupConfiguration.ActiveDirectoryGroups.Any(g => g.ObjectId == resourceId) ? true : false;
        }
    }
}
