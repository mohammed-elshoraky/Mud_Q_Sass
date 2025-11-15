namespace Mud_Q_Sass.API.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }

        // Navigation
        public AppUser User { get; set; } = null!;
    }
}
