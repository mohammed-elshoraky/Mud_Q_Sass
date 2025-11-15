using System.ComponentModel.DataAnnotations;

namespace Mud.Shared.DTOs
{
    public class CreateBranchDto
    {
        [Required] public string Name { get; set; } = null!;
        [Required] public string Address { get; set; } = null!;
        [Required] public string PhoneNumber { get; set; } = null!;
        [Required, EmailAddress] public string Email { get; set; } = null!;
        [Required] public int CompanyId { get; set; }
    }
}
