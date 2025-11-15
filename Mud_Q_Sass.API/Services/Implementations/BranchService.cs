using Microsoft.EntityFrameworkCore;
using Mud_Q_Sass.API.Data;
using Mud.Shared.DTOs;
using Mud_Q_Sass.API.Models;
using Mud_Q_Sass.API.Services.Interfaces.Mud_Q_Sass.API.Services.Interfaces;
using System.Security.Claims;
using Mud.Shared.Common;

namespace Mud_Q_Sass.API.Services.Implementations
{
    public class BranchService : IBranchService
    {
        private readonly SASDbContext _context;
        private readonly IHttpContextAccessor _httpContext;

        public BranchService(SASDbContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
        }

        public async Task<ApiResponse<IEnumerable<BranchDto>>> GetAllAsync(int? companyId = null)
        {
            var userRole = _httpContext.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = int.Parse(_httpContext.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            // لو CompanyAdmin → يشوف فروع شركته بس
            if (userRole == "CompanyAdmin")
            {
                var adminCompanyId = await _context.UserRoles
                    .Where(ur => ur.UserId == userId && ur.Role.Name == "CompanyAdmin")
                    .Select(ur => ur.CompanyId)
                    .FirstOrDefaultAsync();

                companyId = adminCompanyId ?? 0;
            }

            var query = _context.Branches
                .Include(b => b.Company)
                .AsQueryable();

            if (companyId.HasValue && companyId > 0)
                query = query.Where(b => b.CompanyId == companyId.Value);

            var branches = await query
                .Select(b => new BranchDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Address = b.Address,
                    PhoneNumber = b.PhoneNumber,
                    Email = b.Email,
                    CompanyId = b.CompanyId,
                    CompanyName = b.Company.Name,
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            return ApiResponse.Success<IEnumerable<BranchDto>>(branches, "تم جلب الفروع بنجاح");
        }

        public async Task<ApiResponse<BranchDto>> GetByIdAsync(int id)
        {
            var branch = await _context.Branches
                .Include(b => b.Company)
                .Where(b => b.Id == id)
                .Select(b => new BranchDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Address = b.Address,
                    PhoneNumber = b.PhoneNumber,
                    Email = b.Email,
                    CompanyId = b.CompanyId,
                    CompanyName = b.Company.Name,
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (branch == null)
                return ApiResponse.NotFound<BranchDto>("الفرع غير موجود");

            return ApiResponse.Success(branch);
        }

        public async Task<ApiResponse<BranchDto>> CreateAsync(CreateBranchDto dto)
        {
            var company = await _context.Companies.FindAsync(dto.CompanyId);
            if (company == null)
                return ApiResponse.Error<BranchDto>("الشركة غير موجودة", statusCode: 400);

            var branch = new Branch
            {
                Name = dto.Name,
                Address = dto.Address,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                CompanyId = dto.CompanyId,
                Latitude = 0,
                Longitude = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();

            return ApiResponse.Success(new BranchDto
            {
                Id = branch.Id,
                Name = branch.Name,
                Address = branch.Address,
                PhoneNumber = branch.PhoneNumber,
                Email = branch.Email,
                CompanyId = branch.CompanyId,
                CompanyName = company.Name,
                IsActive = branch.IsActive,
                CreatedAt = branch.CreatedAt
            }, "تم إنشاء الفرع بنجاح", 201);
        }

        public async Task<ApiResponse<BranchDto>> UpdateAsync(int id, UpdateBranchDto dto)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
                return ApiResponse.NotFound<BranchDto>("الفرع غير موجود");

            if (!string.IsNullOrEmpty(dto.Name)) branch.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Address)) branch.Address = dto.Address;
            if (!string.IsNullOrEmpty(dto.PhoneNumber)) branch.PhoneNumber = dto.PhoneNumber;
            if (!string.IsNullOrEmpty(dto.Email)) branch.Email = dto.Email;
            if (dto.IsActive.HasValue) branch.IsActive = dto.IsActive.Value;

            branch.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var company = await _context.Companies.FindAsync(branch.CompanyId);

            return ApiResponse.Success(new BranchDto
            {
                Id = branch.Id,
                Name = branch.Name,
                Address = branch.Address,
                PhoneNumber = branch.PhoneNumber,
                Email = branch.Email,
                CompanyId = branch.CompanyId,
                CompanyName = company?.Name ?? "",
                IsActive = branch.IsActive,
                CreatedAt = branch.CreatedAt
            }, "تم تحديث الفرع بنجاح");
        }

        public async Task<ApiResponse<object>> DeleteAsync(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
                return ApiResponse.NotFound<object>("الفرع غير موجود");

            _context.Branches.Remove(branch);
            await _context.SaveChangesAsync();

            return ApiResponse.Success<object>(new { Id = id }, "تم حذف الفرع بنجاح");
        }
    }
}
