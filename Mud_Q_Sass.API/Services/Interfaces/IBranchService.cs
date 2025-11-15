
using Mud.Shared.Common;
using Mud.Shared.DTOs;

namespace Mud_Q_Sass.API.Services.Interfaces
{
    namespace Mud_Q_Sass.API.Services.Interfaces
    {
        public interface IBranchService
        {
            Task<ApiResponse<IEnumerable<BranchDto>>> GetAllAsync(int? companyId = null);
            Task<ApiResponse<BranchDto>> GetByIdAsync(int id);
            Task<ApiResponse<BranchDto>> CreateAsync(CreateBranchDto dto);
            Task<ApiResponse<BranchDto>> UpdateAsync(int id, UpdateBranchDto dto);
            Task<ApiResponse<object>> DeleteAsync(int id);
        }
    }
}
