
using Mud.Shared.Common;
using Mud.Shared.DTOs;

namespace Mud_Q_Sass.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<IEnumerable<UserDto>>> GetAllAsync(int? companyId = null);
        Task<ApiResponse<UserDto>> GetByIdAsync(int id);
        Task<ApiResponse<UserDto>> CreateAsync(CreateUserDto dto);
        Task<ApiResponse<UserDto>> UpdateAsync(int id, UpdateUserDto dto);
        Task<ApiResponse<object>> DeleteAsync(int id);
        Task<ApiResponse<object>> AssignRoleAsync(int userId, string roleName, int? companyId = null, int? branchId = null);
    }
}
