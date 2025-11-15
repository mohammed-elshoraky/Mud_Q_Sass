using Microsoft.EntityFrameworkCore;
using Mud_Q_Sass.API.Data;
using Mud.Shared.DTOs;
using Mud_Q_Sass.API.Services.Interfaces;
using Mud.Shared.Common;

namespace Mud_Q_Sass.API.Services.Implementations
{
    public class UserProfileService : IUserProfileService
    {
        private readonly SASDbContext _context;

        public UserProfileService(SASDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<UserProfileDto>> GetProfileAsync(int userId)
        {
            var user = await _context.AppUsers
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Company)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Branch)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return ApiResponse.NotFound<UserProfileDto>("المستخدم غير موجود");

            var profile = new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                Role = user.UserRoles.FirstOrDefault()?.Role.Name ?? "User",
                CompanyId = user.UserRoles.FirstOrDefault()?.CompanyId,
                CompanyName = user.UserRoles.FirstOrDefault()?.Company?.Name,
                BranchId = user.UserRoles.FirstOrDefault()?.BranchId,
                BranchName = user.UserRoles.FirstOrDefault()?.Branch?.Name,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            return ApiResponse.Success(profile, "تم جلب الملف الشخصي بنجاح");
        }

        public async Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _context.AppUsers.FindAsync(userId);
            if (user == null)
                return ApiResponse.NotFound<UserProfileDto>("المستخدم غير موجود");

            if (!string.IsNullOrEmpty(dto.FullName)) user.FullName = dto.FullName;
            if (!string.IsNullOrEmpty(dto.PhoneNumber)) user.PhoneNumber = dto.PhoneNumber;

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // إعادة جلب البيانات بعد التحديث
            return await GetProfileAsync(userId);
        }
    }
}
