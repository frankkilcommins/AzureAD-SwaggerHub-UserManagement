using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SwaggerHubDemo.Models;
using SwaggerHubDemo.Models.SwaggerHub;

namespace SwaggerHubDemo.Services
{
    public interface ISwaggerHubUserManagementService
    {
        Task<MemberDetail> GetMemberByEmail(ILogger logger, ActiveDirectoryGroup group, string email);
        Task CreateMember(ILogger logger, ActiveDirectoryGroup group, string firstName, string lastName, string email);
        Task<PatchedMember> UpdateMember(ILogger logger, ActiveDirectoryGroup group, PatchMemberRequest content);
        Task DeleteMember(ILogger logger, ActiveDirectoryGroup group, string email);
    }
}