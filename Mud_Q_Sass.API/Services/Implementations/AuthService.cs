using Microsoft.IdentityModel.Tokens;
using Mud_Q_Sass.API.Data;
using Mud.Shared.DTOs;
using Mud_Q_Sass.API.Models;
using Mud_Q_Sass.API.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Mud.Shared.Common;
using ApiResponse = Mud.Shared.Common.ApiResponse;

namespace Mud_Q_Sass.API.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly SASDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(SASDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<Mud.Shared.Common.ApiResponse<AuthResponse>> LoginAsync(LoginDto dto)
        {
            var user = _context.AppUsers
                .FirstOrDefault(u => u.Username == dto.Username);

            if (user == null || !VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt))
                return ApiResponse.Unauthorized<AuthResponse>("اسم المستخدم أو كلمة المرور غير صحيحة");

            var (accessToken, refreshToken) = GenerateTokens(user);
            await SaveRefreshToken(user.Id, refreshToken);

            return ApiResponse.Success(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                Role = GetUserRole(user.Id),
                Username = user.Username
            }, "تم تسجيل الدخول بنجاح");
        }

        public async Task<Mud.Shared.Common.ApiResponse<AuthResponse>> RegisterAsync(RegisterDto dto)
        {
            if (_context.AppUsers.Any(u => u.Username == dto.Username || u.Email == dto.Email))
                return ApiResponse.Error<AuthResponse>("المستخدم موجود بالفعل", statusCode: 409);

            var (hash, salt) = HashPassword(dto.Password);
            var user = new AppUser
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                FullName = dto.FullName,
                IsActive = true
            };

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();

            // تعيين كـ User عادي
            var role = _context.Roles.First(r => r.Name == "User");
            _context.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                CompanyId = null,
                BranchId = null
            });
            await _context.SaveChangesAsync();

            var (accessToken, refreshToken) = GenerateTokens(user);
            await SaveRefreshToken(user.Id, refreshToken);

            return ApiResponse.Success(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                Role = "User",
                Username = user.Username
            }, "تم التسجيل بنجاح", 201);
        }

        public async Task<Mud.Shared.Common.ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshToken)
        {
            var stored = _context.RefreshTokens
                .FirstOrDefault(t => t.Token == refreshToken && t.ExpiresAt > DateTime.UtcNow && !t.IsRevoked);

            if (stored == null)
                return ApiResponse.Unauthorized<AuthResponse>("الـ Refresh Token غير صالح");

            var user = _context.AppUsers.Find(stored.UserId);
            if (user == null) return ApiResponse.Unauthorized<AuthResponse>("المستخدم غير موجود");

            var (newAccess, newRefresh) = GenerateTokens(user);
            stored.Token = newRefresh;
            stored.ExpiresAt = DateTime.UtcNow.AddDays(7);
            stored.IsRevoked = false;
            await _context.SaveChangesAsync();

            return ApiResponse.Success(new AuthResponse
            {
                AccessToken = newAccess,
                RefreshToken = newRefresh,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                Role = GetUserRole(user.Id),
                Username = user.Username
            });
        }

        public async Task<Mud.Shared.Common.ApiResponse<object>> LogoutAsync(string refreshToken)
        {
            var token = _context.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);
            if (token != null)
            {
                token.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
            return ApiResponse.Success<object>(null, "تم تسجيل الخروج");
        }

        // --- Helper Methods ---
        private (byte[] hash, byte[] salt) HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(32);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 10000, HashAlgorithmName.SHA256, 32);
            return (hash, salt);
        }

        private bool VerifyPassword(string password, byte[] hash, byte[] salt)
        {
            var computed = Rfc2898DeriveBytes.Pbkdf2(password, salt, 10000, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(hash, computed);
        }

        private (string access, string refresh) GenerateTokens(AppUser user)
        {
            var role = GetUserRole(user.Id);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, role),  // ⬅️ للـ Backend Authorization
                new Claim("role", role)            // ⬅️ للـ JWT Standard + Frontend
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var accessToken = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            var refreshToken = Guid.NewGuid().ToString();

            return (new JwtSecurityTokenHandler().WriteToken(accessToken), refreshToken);
        }

        private string GetUserRole(int userId)
        {
            return _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .FirstOrDefault() ?? "User";
        }

        private async Task SaveRefreshToken(int userId, string token)
        {
            var existing = _context.RefreshTokens.FirstOrDefault(t => t.UserId == userId);

            if (existing != null)
            {
                existing.Token = token;
                existing.ExpiresAt = DateTime.UtcNow.AddDays(7);
                existing.IsRevoked = false;
            }
            else
            {
                _context.RefreshTokens.Add(new RefreshToken
                {
                    UserId = userId,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}