using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
        public async Task<MemberDetail> GetMemberByEmail(ActiveDirectoryGroup group, string email)
        {
            var organizationId = group.Organizations.FirstOrDefault().Name;
            var repositoryResult = await _swaggerHubRepository.Get<MembersResponse>($"/orgs/{organizationId}/members", $"q={email}");

            if(repositoryResult.IsSuccessCode && repositoryResult.Data != null)
            {
                //ToDo - Use Auto Mapper
                return repositoryResult.Data.Items.FirstOrDefault();
            }

            return null;
        }

        public async Task<NewMember> CreateMember(ActiveDirectoryGroup group, string firstName, string lastName, string email)
        {
            // ToDo - loop over organizations (supporting more complex setups)

            var organizationId = group.Organizations.FirstOrDefault().Name;
            var existingUserResult = await _swaggerHubRepository.Get<MembersResponse>($"/orgs/{organizationId}/members", $"q={email}");

            if(existingUserResult.IsSuccessCode && existingUserResult.Data != null && existingUserResult.Data.Items.Count == 1)
            {
                // Prepare PATCH request
                PatchMemberRequest patchReq = new PatchMemberRequest()
                {
                    Members = new Collection<ModifiedMember>{ new ModifiedMember() { Email = email, Role = group.SwaggerHubRole}}
                };

                var patchResult = await _swaggerHubRepository.Patch<Collection<PatchedMember>>($"/orgs/{organizationId}/members", patchReq);

                if(patchResult.IsSuccessCode && patchResult.Data != null)
                {
                    //ToDo - use auto mapper
                    PatchedMember patchedMember = patchResult.Data.FirstOrDefault();
                    return new NewMember() { Email = patchedMember.Email, Status = patchedMember.Status};
                }
                else
                {
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
                //ToDo - GET LOGGER IN HERE
                //ToDo - Use Auto Mapper
                return repositoryResult.Data.Invited.FirstOrDefault();
            }

            return null;
        }

        public async Task<PatchedMember> UpdateMember(ActiveDirectoryGroup group, PatchMemberRequest content)
        {
            var organizationId = group.Organizations.FirstOrDefault().Name;
            var repositoryResult = await _swaggerHubRepository.Patch<Collection<PatchedMember>>($"/orgs/{organizationId}/members", content);

            if(repositoryResult.IsSuccessCode && repositoryResult.Data != null)
            {
                //ToDo - Use Auto Mapper
                return repositoryResult.Data.FirstOrDefault();
            }

            return null;
        }

        public async Task<DeletedMember> DeleteMember(ActiveDirectoryGroup group, string email)
        {
            var organizationId = group.Organizations.FirstOrDefault().Name;
            var repositoryResult = await _swaggerHubRepository.Delete<Collection<DeletedMember>>($"/orgs/{organizationId}/members", $"user={email}");

            if(repositoryResult.IsSuccessCode && repositoryResult.Data != null)
            {
                //ToDo - Use Auto Mapper
                return repositoryResult.Data.FirstOrDefault();
            }

            return null;
        }
    }
}