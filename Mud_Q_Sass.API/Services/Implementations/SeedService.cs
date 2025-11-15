using Mud_Q_Sass.API.Data;
using Mud_Q_Sass.API.Models;
using System.Security.Cryptography;

namespace Mud_Q_Sass.API.Services.Implementations
{
    public class SeedService
    {
        private readonly SASDbContext _context;

        public SeedService(SASDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            await SeedRolesAsync();
            await SeedSuperAdminAsync();
        }

        private async Task SeedRolesAsync()
        {
            var roles = new[]
            {
                new Role { Name = "SuperAdmin", Description = "مدير النظام الكامل", IsSystemRole = true },
                new Role { Name = "CompanyAdmin", Description = "مدير الشركة", IsSystemRole = true },
                new Role { Name = "BranchAdmin", Description = "مدير الفرع", IsSystemRole = true },
                new Role { Name = "User", Description = "مستخدم عادي", IsSystemRole = true }
            };

            foreach (var role in roles)
            {
                if (!_context.Roles.Any(r => r.Name == role.Name))
                {
                    _context.Roles.Add(role);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task SeedSuperAdminAsync()
        {
            var username = "superadmin";
            var email = "superadmin@sas.com";

            if (_context.AppUsers.Any(u => u.Username == username || u.Email == email))
                return;

            var (hash, salt) = HashPassword("Admin@123");

            var superAdmin = new AppUser
            {
                Username = username,
                Email = email,
                PasswordHash = hash,
                PasswordSalt = salt,
                FullName = "Super Administrator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.AppUsers.Add(superAdmin);
            await _context.SaveChangesAsync();

            var superAdminRole = _context.Roles.First(r => r.Name == "SuperAdmin");
            _context.UserRoles.Add(new UserRole
            {
                UserId = superAdmin.Id,
                RoleId = superAdminRole.Id
            });

            await _context.SaveChangesAsync();
        }

        private (byte[] hash, byte[] salt) HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(32);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 10000, HashAlgorithmName.SHA256, 32);
            return (hash, salt);
        }
    }
}
