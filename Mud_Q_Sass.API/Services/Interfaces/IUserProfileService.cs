
using Mud.Shared.Common;
using Mud.Shared.DTOs;

namespace Mud_Q_Sass.API.Services.Interfaces
{
    public interface IUserProfileService
    {
        Task<ApiResponse<UserProfileDto>> GetProfileAsync(int userId);
        Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileDto dto);
    }
}
