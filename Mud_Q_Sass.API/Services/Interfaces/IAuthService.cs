using Mud.Shared.Common;
using Mud.Shared.DTOs;


namespace Mud_Q_Sass.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<Mud.Shared.Common.ApiResponse<AuthResponse>> LoginAsync(LoginDto dto);
        Task<Mud.Shared.Common.ApiResponse<AuthResponse>> RegisterAsync(RegisterDto dto);
        Task<Mud.Shared.Common.ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshToken);
        Task<Mud.Shared.Common.ApiResponse<object>> LogoutAsync(string refreshToken);
    }

    
}
