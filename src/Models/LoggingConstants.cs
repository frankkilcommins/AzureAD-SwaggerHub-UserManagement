namespace SwaggerHubDemo.Models
{
    internal static class LoggingConstants
    {
        // Template for consisted structured logging accross multiple functions, each field is described below: 
        // EventDescription is a short description of the Event being logged. 
        // EntityType: Business Entity Type being processed: e.g. Group, User, SubscriptionManagement, etc.
        // EntityId: Id of the Business Entity being processed: e.g. Object Id of the user, or group, or subscription. 
        // RelatedEntityType: Related Business Entity Type being processed: e.g. Group, User, SubscriptionManagement, etc. Gives additional context
        // RelatedEntityId: Id of the Business Entity being processed: e.g. Object Id of the user, or group, or subscription. 
        // CorrelationId: Unique identifier of the message that can be processed by more than one component. 
        // Description: A detailed description of the log event. 
        internal const string Template = "{EventDescription}, {EntityType}, {EntityId}, {RelatedEntityType}, {RelatedEntityId}, {CorrelationId}, {Description}";

        internal enum EntityType
        {
            UserManagement,
            SubscriptionManagement
        }

        /// <summary>
        /// Enumeration of all different EventId that can be used for logging
        /// </summary>
        internal enum EventId
        {
            SwaggerHub_UserManagement_Get_User_Failed = 1000,
            SwaggerHub_UserManagement_Patch_User_Failed = 1001,
            SwaggerHub_UserManagement_Post_User_Failed = 1002,
            SwaggerHub_UserManagement_Delete_User_Failed = 1003,
            Graph_Subscription_Renewal_Failed = 1100
        }

    }
}