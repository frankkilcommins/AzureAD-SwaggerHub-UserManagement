using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwaggerHubDemo.Models;
using SwaggerHubDemo.Models.SwaggerHub;
using SwaggerHubDemo.Repositories;

namespace SwaggerHubDemo.Services
{
    public static class SwaggerHubUserManagementServiceExtensions
    {
        public static void AddSwaggerHubUserManagementService(this IServiceCollection services)
        {
            services.AddScoped<ISwaggerHubUserManagementService, SwaggerHubUserManagementService>();
        }
    }
    public class SwaggerHubUserManagementService : ISwaggerHubUserManagementService
    {
        private readonly ISwaggerHubRepository _swaggerHubRepository;

        public SwaggerHubUserManagementService(ISwaggerHubRepository swaggerHubRepository)
        {
            _swaggerHubRepository = swaggerHubRepository;
        }
        public async Task<MemberDetail> GetMemberByEmail(ILogger logger, ActiveDirectoryGroup group, string email)
        {
            var organizationId = group.Organizations.FirstOrDefault().Name;
            var repositoryResult = await _swaggerHubRepository.Get<MembersResponse>($"/orgs/{organizationId}/members", $"q={email}");

            if(repositoryResult.IsSuccessCode && repositoryResult.Data != null)
            {
                return repositoryResult.Data.Items.FirstOrDefault();
            }

            logger.LogWarning(new EventId((int)LoggingConstants.EventId.SwaggerHub_UserManagement_Get_User_Failed),
                LoggingConstants.Template,
                LoggingConstants.EventId.SwaggerHub_UserManagement_Get_User_Failed.ToString(),
                "user",
                email,
                "organization",
                organizationId,
                "",
                $"User Management API returned an error (status code: {repositoryResult.StatusCode}). User {email} could not be retrieved from organization {organizationId}. Error Details: id: {repositoryResult.Error.Id}, message {repositoryResult.Error.Message} ");


            return null;
        }

        public async Task CreateMember(ILogger logger, ActiveDirectoryGroup group, string firstName, string lastName, string email)
        {
            // loop over organizations (supporting more complex setups)
            foreach(var org in group.Organizations)
            {
                var existingUserResult = await _swaggerHubRepository.Get<MembersResponse>($"/orgs/{org.Name}/members", $"q={email}");

                if(existingUserResult.IsSuccessCode && existingUserResult.Data != null && existingUserResult.Data.Items.Count == 1)
                {
                    logger.LogInformation($"User {email} already exists, thus patching user...");
                    
                    // Prepare PATCH request (group assignment is a race....last assignment sets the user role in organization)
                    PatchMemberRequest patchReq = new PatchMemberRequest()
                    {
                        Members = new Collection<ModifiedMember>{ new ModifiedMember() { Email = email, Role = group.SwaggerHubRole}}
                    };

                    var patchResult = await _swaggerHubRepository.Patch<Collection<PatchedMember>>($"/orgs/{org.Name}/members", patchReq);

                    if(patchResult.IsSuccessCode && patchResult.Data != null)
                    {
                        PatchedMember patchedMember = patchResult.Data.FirstOrDefault();
                        logger.LogInformation($"Updated SwaggerHub User! Details: [orgName: {org.Name}, email: {patchedMember.Email}, status: {patchedMember.Status}]");
                    }
                    else
                    {
                        logger.LogError(new EventId((int)LoggingConstants.EventId.SwaggerHub_UserManagement_Patch_User_Failed),
                            LoggingConstants.Template,
                            LoggingConstants.EventId.SwaggerHub_UserManagement_Patch_User_Failed.ToString(),
                            "user",
                            email,
                            "organization",
                            org.Name,
                            "",
                            $"User Management API returned an error (status code: {patchResult.StatusCode}). User {email} could not be modified in organization {org.Name}. Error Details: id: {patchResult.Error.Id}, message {patchResult.Error.Message} ");
                    }
                }
            
                //user does not exist so prepare new user request
                NewMemberRequest requestObj = new NewMemberRequest()
                {
                    Members = new Collection<Member>{ new Member() {Email = email, FirstName = firstName, LastName = lastName, Role = group.SwaggerHubRole }}
                };

                var repositoryResult = await _swaggerHubRepository.Post<NewMemberResponse>($"/orgs/{org.Name}/members", requestObj);
                
                if(repositoryResult.IsSuccessCode && repositoryResult.Data != null)
                {
                    var createdUser = repositoryResult.Data.Invited.FirstOrDefault();
                    logger.LogInformation($"Created SwaggerHub User! Details: [orgName: {org.Name}, firstName: {firstName}, lastName: {lastName}, email: {createdUser.Email}, status: {createdUser.Status}]");
                }
                else
                {
                    logger.LogError(new EventId((int)LoggingConstants.EventId.SwaggerHub_UserManagement_Post_User_Failed),
                        LoggingConstants.Template,
                        LoggingConstants.EventId.SwaggerHub_UserManagement_Post_User_Failed.ToString(),
                        "user",
                        email,
                        "organization",
                        org.Name,
                        "",
                        $"User Management API returned an error (status code: {repositoryResult.StatusCode}). User {email} could not be created in organization {org.Name}. Error Details: id: {repositoryResult.Error.Id}, message {repositoryResult.Error.Message} ");

                }
            }
        }

        public async Task<PatchedMember> UpdateMember(ILogger logger, ActiveDirectoryGroup group, PatchMemberRequest content)
        {
            var organizationId = group.Organizations.FirstOrDefault().Name;
            var repositoryResult = await _swaggerHubRepository.Patch<Collection<PatchedMember>>($"/orgs/{organizationId}/members", content);

            if(repositoryResult.IsSuccessCode && repositoryResult.Data != null)
            {
                return repositoryResult.Data.FirstOrDefault();
            }

            logger.LogError(new EventId((int)LoggingConstants.EventId.SwaggerHub_UserManagement_Patch_User_Failed),
                LoggingConstants.Template,
                LoggingConstants.EventId.SwaggerHub_UserManagement_Patch_User_Failed.ToString(),
                "user",
                content.Members.FirstOrDefault().Email,
                "organization",
                organizationId,
                "",
                $"User Management API returned an error (status code: {repositoryResult.StatusCode}). User {content.Members.FirstOrDefault().Email} could not be modified in organization {organizationId}. Error Details: id: {repositoryResult.Error.Id}, message {repositoryResult.Error.Message} ");

            return null;
        }

        public async Task DeleteMember(ILogger logger, ActiveDirectoryGroup group, string email)
        {
            foreach(var org in group.Organizations)
            {
                // Can user be deleted
                if(await IsUserRemovalSafe(logger, org.Name, email, group))
                {
                var repositoryResult = await _swaggerHubRepository.Delete<Collection<DeletedMember>>($"/orgs/{org.Name}/members", $"user={email}");

                    if(repositoryResult.IsSuccessCode && repositoryResult.Data != null)
                    {
                        var deletedMember = repositoryResult.Data.FirstOrDefault();
                        logger.LogInformation($"Deleted SwaggerHub User! Details:[Org: {org.Name}, username: {deletedMember.Username}, email: {deletedMember.Email}, status: {deletedMember.Status}]");
                    }
                    else 
                    {
                        logger.LogError(new EventId((int)LoggingConstants.EventId.SwaggerHub_UserManagement_Delete_User_Failed),
                            LoggingConstants.Template,
                            LoggingConstants.EventId.SwaggerHub_UserManagement_Delete_User_Failed.ToString(),
                            "user",
                            email,
                            "organization",
                            org.Name,
                            "",
                            $"User Management API returned an error (status code: {repositoryResult.StatusCode}). User {email} could not be deleted from organization {org.Name}. Error Details: id: {repositoryResult.Error.Id}, message {repositoryResult.Error.Message} ");
                    }
                }
            }
        }

        //This is slightly crude but helps to reduce likelihood of user removal if there's a multi-group assignment for different roles
        private async Task<bool> IsUserRemovalSafe(ILogger logger, string organizationId, string email, ActiveDirectoryGroup group)
        {            
            // Get user details
            var existingUserResult = await _swaggerHubRepository.Get<MembersResponse>($"/orgs/{organizationId}/members", $"q={email}");

            // Check if user role matches that of the removed group assignment
            if(existingUserResult.IsSuccessCode && existingUserResult.Data != null && existingUserResult.Data.Items.Count == 1)
            {
                var existingUser = existingUserResult.Data.Items.FirstOrDefault();

                if(string.Equals(existingUser.Role, group.SwaggerHubRole, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    logger.LogInformation($"Delete NOT processed!. User [{email}] has role [{existingUser.Role}] in [{organizationId}] but request is for role [{group.SwaggerHubRole}]....we assume multiple groups are assigned");
                }
            }
            else
            {
                logger.LogInformation($"Delete NOT processed!. Details: User [{email}] does not exist in organization [{organizationId}].");
            }

            return false;           
        }
    }
}