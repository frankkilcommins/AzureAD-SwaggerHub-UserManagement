using System.Threading.Tasks;
using SwaggerHubDemo.Models;
using SwaggerHubDemo.Models.SwaggerHub;

namespace SwaggerHubDemo.Services
{
    public interface ISwaggerHubUserManagementService
    {
        Task<MemberDetail> GetMemberByEmail(ActiveDirectoryGroup group, string email);
        Task<NewMember> CreateMember(ActiveDirectoryGroup group, string firstName, string lastName, string email);
        Task<PatchedMember> UpdateMember(ActiveDirectoryGroup group, PatchMemberRequest content);
        Task<DeletedMember> DeleteMember(ActiveDirectoryGroup group, string email);
    }
}