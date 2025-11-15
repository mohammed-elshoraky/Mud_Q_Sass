using System.ComponentModel.DataAnnotations;

namespace Mud.Shared.DTOs
{
    public class CreateUserDto
    {
        [Required] public string Username { get; set; } = null!;
        [Required, EmailAddress] public string Email { get; set; } = null!;
        [Required, MinLength(6)] public string Password { get; set; } = null!;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public int? CompanyId { get; set; }
        public int? BranchId { get; set; }
    }
}
