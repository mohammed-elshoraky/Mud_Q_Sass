

using Mud.Shared.Common;
using Mud.Shared.DTOs;

namespace Mud_Q_Sass.API.Services.Interfaces
{
    public interface ICompanyService
    {
        Task<ApiResponse<List<CompanyDto>>> GetAllAsync();
        Task<ApiResponse<CompanyDto>> GetByIdAsync(int id);
        Task<ApiResponse<CompanyDto>> CreateAsync(CreateCompanyDto dto);
        Task<ApiResponse<object>> UpdateAsync(int id, UpdateCompanyDto dto);
        Task<ApiResponse<object>> DeleteAsync(int id);
    }
}
