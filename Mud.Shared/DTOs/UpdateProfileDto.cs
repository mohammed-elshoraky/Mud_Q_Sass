namespace Mud.Shared.DTOs
{
    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }

        // لا يُسمح بتغيير الـ Username أو الـ Email هنا (للأمان)
        // لو عاوز تغيّرهم → اعمل API منفصل بـ Two-Factor
    }
}
