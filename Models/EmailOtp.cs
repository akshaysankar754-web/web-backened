namespace InternshipPortalApi.Models
{
    public class EmailOtp
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string OtpCode { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; }

        public User? User { get; set; }
    }
}
