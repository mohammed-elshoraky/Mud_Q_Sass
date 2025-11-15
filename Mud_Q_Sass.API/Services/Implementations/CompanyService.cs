// Services/Implementations/CompanyService.cs
using Microsoft.EntityFrameworkCore;
using Mud_Q_Sass.API.Data;
using Mud_Q_Sass.API.Services.Interfaces;
using Mud_Q_Sass.API.Models;
using Mud.Shared.Common;
using Mud.Shared.DTOs;

namespace Mud_Q_Sass.API.Services.Implementations
{
    public class CompanyService : ICompanyService
    {
        private readonly SASDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CompanyService(SASDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<ApiResponse<List<CompanyDto>>> GetAllAsync()
        {
            try
            {
                var companies = await _context.Companies
                    .Select(c => new CompanyDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        IsActive = c.IsActive,
                        LogoPath = c.LogoPath,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();

                return ApiResponse.Success(companies, "تم جلب الشركات بنجاح");
            }
            catch (Exception ex)
            {
                return ApiResponse.ServerError<List<CompanyDto>>("خطأ في جلب الشركات: " + ex.Message);
            }
        }

        public async Task<ApiResponse<CompanyDto>> GetByIdAsync(int id)
        {
            try
            {
                var company = await _context.Companies.FindAsync(id);
                if (company == null)
                    return ApiResponse.NotFound<CompanyDto>("الشركة غير موجودة");

                var dto = new CompanyDto
                {
                    Id = company.Id,
                    Name = company.Name,
                    IsActive = company.IsActive,
                    LogoPath = company.LogoPath,
                    CreatedAt = company.CreatedAt
                };

                return ApiResponse.Success(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse.ServerError<CompanyDto>("خطأ في جلب الشركة: " + ex.Message);
            }
        }

        public async Task<ApiResponse<CompanyDto>> CreateAsync(CreateCompanyDto dto)
        {
            try
            {
                string? logoPath = null;

                if (dto.Logo != null)
                {
                    logoPath = await SaveLogoAsync(dto.Logo);
                }

                var company = new Company
                {
                    Name = dto.Name,
                    IsActive = dto.IsActive,
                    LogoPath = logoPath,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                var result = new CompanyDto
                {
                    Id = company.Id,
                    Name = company.Name,
                    IsActive = company.IsActive,
                    LogoPath = company.LogoPath,
                    CreatedAt = company.CreatedAt
                };

                return ApiResponse.Success(result, "تم إنشاء الشركة بنجاح", 201);
            }
            catch (Exception ex)
            {
                return ApiResponse.ServerError<CompanyDto>("خطأ في إنشاء الشركة: " + ex.Message);
            }
        }

        public async Task<ApiResponse<object>> UpdateAsync(int id, UpdateCompanyDto dto)
        {
            try
            {
                var company = await _context.Companies.FindAsync(id);
                if (company == null)
                    return ApiResponse.NotFound<object>("الشركة غير موجودة");

                company.Name = dto.Name;
                company.IsActive = dto.IsActive;

                if (dto.Logo != null)
                {
                    // حذف الشعار القديم
                    if (!string.IsNullOrEmpty(company.LogoPath))
                        DeleteLogo(company.LogoPath);

                    company.LogoPath = await SaveLogoAsync(dto.Logo);
                }

                await _context.SaveChangesAsync();

                return ApiResponse.Success<object>(null, "تم تعديل الشركة بنجاح");
            }
            catch (Exception ex)
            {
                return ApiResponse.ServerError<object>("خطأ في التعديل: " + ex.Message);
            }
        }

        public async Task<ApiResponse<object>> DeleteAsync(int id)
        {
            try
            {
                var company = await _context.Companies.FindAsync(id);
                if (company == null)
                    return ApiResponse.NotFound<object>("الشركة غير موجودة");

                if (!string.IsNullOrEmpty(company.LogoPath))
                    DeleteLogo(company.LogoPath);

                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();

                return ApiResponse.Success<object>(null, "تم حذف الشركة بنجاح");
            }
            catch (Exception ex)
            {
                return ApiResponse.ServerError<object>("خطأ في الحذف: " + ex.Message);
            }
        }

        private async Task<string> SaveLogoAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "logos");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/logos/{uniqueFileName}";
        }

        private void DeleteLogo(string logoPath)
        {
            var filePath = Path.Combine(_env.WebRootPath, logoPath.TrimStart('/'));
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}