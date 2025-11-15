using Microsoft.EntityFrameworkCore;
using Mud.Shared.Common;
using Mud.Shared.DTOs;
using Mud_Q_Sass.API.Data;
using Mud_Q_Sass.API.Models;
using Mud_Q_Sass.API.Services.Interfaces;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Mud_Q_Sass.API.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly SASDbContext _context;
        private readonly IHttpContextAccessor _httpContext;

        public UserService(SASDbContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
        }

        public async Task<ApiResponse<IEnumerable<UserDto>>> GetAllAsync(int? companyId = null)
        {
            var currentUserRole = _httpContext.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
            var currentUserId = int.Parse(_httpContext.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            // لو CompanyAdmin → يشوف مستخدمين شركته بس
            if (currentUserRole == "CompanyAdmin")
            {
                var adminCompanyId = await _context.UserRoles
                    .Where(ur => ur.UserId == currentUserId && ur.Role.Name == "CompanyAdmin")
                    .Select(ur => ur.CompanyId)
                    .FirstOrDefaultAsync();

                companyId = adminCompanyId;
            }

            var query = _context.AppUsers
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Company)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Branch)
                .AsQueryable();

            if (companyId.HasValue)
                query = query.Where(u => u.UserRoles.Any(ur => ur.CompanyId == companyId));

            var users = await query
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    IsActive = u.IsActive,
                    Role = u.UserRoles.FirstOrDefault()!.Role.Name,
                    CompanyId = u.UserRoles.FirstOrDefault()!.CompanyId,
                    CompanyName = u.UserRoles.FirstOrDefault()!.Company!.Name,
                    BranchId = u.UserRoles.FirstOrDefault()!.BranchId,
                    BranchName = u.UserRoles.FirstOrDefault()!.Branch != null ? u.UserRoles.FirstOrDefault()!.Branch.Name : null,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return ApiResponse.Success<IEnumerable<UserDto>>(users, "تم جلب المستخدمين بنجاح");
        }

        public async Task<ApiResponse<UserDto>> GetByIdAsync(int id)
        {
            var user = await _context.AppUsers
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Company)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Branch)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return ApiResponse.NotFound<UserDto>("المستخدم غير موجود");

            var userDto = new UserDto
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
                CreatedAt = user.CreatedAt
            };

            return ApiResponse.Success(userDto);
        }

        public async Task<ApiResponse<UserDto>> CreateAsync(CreateUserDto dto)
        {
            if (await _context.AppUsers.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email))
                return ApiResponse.Error<UserDto>("اسم المستخدم أو البريد موجود بالفعل", statusCode: 409);

            var (hash, salt) = HashPassword(dto.Password);

            var user = new AppUser
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();

            // تعيين الدور الافتراضي: User
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User")
                       ?? await _context.Roles.FirstAsync(r => r.Name == "User");

            _context.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                CompanyId = dto.CompanyId,
                BranchId = dto.BranchId
            });

            await _context.SaveChangesAsync();

            return ApiResponse.Success(new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                Role = "User",
                CompanyId = dto.CompanyId,
                CompanyName = dto.CompanyId != null ? (await _context.Companies.FindAsync(dto.CompanyId))?.Name : null,
                BranchId = dto.BranchId,
                BranchName = dto.BranchId != null ? (await _context.Branches.FindAsync(dto.BranchId))?.Name : null,
                CreatedAt = user.CreatedAt
            }, "تم إنشاء المستخدم بنجاح", 201);
        }

        public async Task<ApiResponse<UserDto>> UpdateAsync(int id, UpdateUserDto dto)
        {
            var user = await _context.AppUsers.FindAsync(id);
            if (user == null)
                return ApiResponse.NotFound<UserDto>("المستخدم غير موجود");

            if (!string.IsNullOrEmpty(dto.Username)) user.Username = dto.Username;
            if (!string.IsNullOrEmpty(dto.Email)) user.Email = dto.Email;
            if (!string.IsNullOrEmpty(dto.FullName)) user.FullName = dto.FullName;
            if (!string.IsNullOrEmpty(dto.PhoneNumber)) user.PhoneNumber = dto.PhoneNumber;
            if (dto.IsActive.HasValue) user.IsActive = dto.IsActive.Value;

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var userRole = await _context.UserRoles
                .Include(ur => ur.Company)
                .Include(ur => ur.Branch)
                .FirstOrDefaultAsync(ur => ur.UserId == id);

            return ApiResponse.Success(new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                Role = userRole?.Role.Name ?? "User",
                CompanyId = userRole?.CompanyId,
                CompanyName = userRole?.Company?.Name,
                BranchId = userRole?.BranchId,
                BranchName = userRole?.Branch?.Name,
                CreatedAt = user.CreatedAt
            }, "تم تحديث المستخدم بنجاح");
        }

        public async Task<ApiResponse<object>> DeleteAsync(int id)
        {
            var user = await _context.AppUsers.FindAsync(id);
            if (user == null)
                return ApiResponse.NotFound<object>("المستخدم غير موجود");

            _context.AppUsers.Remove(user);
            await _context.SaveChangesAsync();

            return ApiResponse.Success<object>(new { Id = id }, "تم حذف المستخدم بنجاح");
        }

        public async Task<ApiResponse<object>> AssignRoleAsync(int userId, string roleName, int? companyId = null, int? branchId = null)
        {
            var user = await _context.AppUsers.FindAsync(userId);
            if (user == null)
                return ApiResponse.NotFound<object>("المستخدم غير موجود");

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
                return ApiResponse.Error<object>("الدور غير موجود", statusCode: 400);

            var existing = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId);
            if (existing != null)
            {
                existing.RoleId = role.Id;
                existing.CompanyId = companyId;
                existing.BranchId = branchId;
            }
            else
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = role.Id,
                    CompanyId = companyId,
                    BranchId = branchId
                });
            }

            await _context.SaveChangesAsync();
            return ApiResponse.Success<object>(null, $"تم تعيين الدور {roleName} للمستخدم");
        }

        // Helper
        private (byte[] hash, byte[] salt) HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(32);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 10000, HashAlgorithmName.SHA256, 32);
            return (hash, salt);
        }
    }
}
