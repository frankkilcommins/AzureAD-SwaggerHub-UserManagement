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

        public async Task<NewMember> CreateMember(ILogger logger, ActiveDirectoryGroup group, string firstName, string lastName, string email)
        {
            // ToDo - loop over organizations (supporting more complex setups)

            var organizationId = group.Organizations.FirstOrDefault().Name;
            var existingUserResult = await _swaggerHubRepository.Get<MembersResponse>($"/orgs/{organizationId}/members", $"q={email}");

            if(existingUserResult.IsSuccessCode && existingUserResult.Data != null && existingUserResult.Data.Items.Count == 1)
            {
                logger.LogInformation($"User {email} already exists, thus patching user...");
                // Prepare PATCH request
                PatchMemberRequest patchReq = new PatchMemberRequest()
                {
                    Members = new Collection<ModifiedMember>{ new ModifiedMember() { Email = email, Role = group.SwaggerHubRole}}
                };

                var patchResult = await _swaggerHubRepository.Patch<Collection<PatchedMember>>($"/orgs/{organizationId}/members", patchReq);

                if(patchResult.IsSuccessCode && patchResult.Data != null)
                {
                    PatchedMember patchedMember = patchResult.Data.FirstOrDefault();
                    return new NewMember() { Email = patchedMember.Email, Status = patchedMember.Status};
                }
                else
                {
                    logger.LogError(new EventId((int)LoggingConstants.EventId.SwaggerHub_UserManagement_Patch_User_Failed),
                        LoggingConstants.Template,
                        LoggingConstants.EventId.SwaggerHub_UserManagement_Patch_User_Failed.ToString(),
                        "user",
                        email,
                        "organization",
                        organizationId,
                        "",
                        $"User Management API returned an error (status code: {patchResult.StatusCode}). User {email} could not be modified in organization {organizationId}. Error Details: id: {patchResult.Error.Id}, message {patchResult.Error.Message} ");

                    return null;
                }
            }
                        
            NewMemberRequest requestObj = new NewMemberRequest()
            {
                Members = new Collection<Member>{ new Member() {Email = email, FirstName = firstName, LastName = lastName, Role = group.SwaggerHubRole }}
            };

            var repositoryResult = await _swaggerHubRepository.Post<NewMemberResponse>($"/orgs/{organizationId}/members", requestObj);
            
            if(repositoryResult.IsSuccessCode && repositoryResult.Data != null)
            {
                return repositoryResult.Data.Invited.FirstOrDefault();
            }

            logger.LogError(new EventId((int)LoggingConstants.EventId.SwaggerHub_UserManagement_Post_User_Failed),
                LoggingConstants.Template,
                LoggingConstants.EventId.SwaggerHub_UserManagement_Post_User_Failed.ToString(),
                "user",
                email,
                "organization",
                organizationId,
                "",
                $"User Management API returned an error (status code: {repositoryResult.StatusCode}). User {email} could not be created in organization {organizationId}. Error Details: id: {repositoryResult.Error.Id}, message {repositoryResult.Error.Message} ");

            return null;
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

        public async Task<DeletedMember> DeleteMember(ILogger logger, ActiveDirectoryGroup group, string email)
        {
            var organizationId = group.Organizations.FirstOrDefault().Name;
            var repositoryResult = await _swaggerHubRepository.Delete<Collection<DeletedMember>>($"/orgs/{organizationId}/members", $"user={email}");

            if(repositoryResult.IsSuccessCode && repositoryResult.Data != null)
            {
                return repositoryResult.Data.FirstOrDefault();
            }

            logger.LogError(new EventId((int)LoggingConstants.EventId.SwaggerHub_UserManagement_Delete_User_Failed),
                LoggingConstants.Template,
                LoggingConstants.EventId.SwaggerHub_UserManagement_Delete_User_Failed.ToString(),
                "user",
                email,
                "organization",
                organizationId,
                "",
                $"User Management API returned an error (status code: {repositoryResult.StatusCode}). User {email} could not be deleted from organization {organizationId}. Error Details: id: {repositoryResult.Error.Id}, message {repositoryResult.Error.Message} ");

            return null;
        }
    }
}