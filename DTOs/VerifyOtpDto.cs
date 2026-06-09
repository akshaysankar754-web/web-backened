namespace InternshipPortalApi.DTOs
{
    public class VerifyOtpDto
    {
        public int UserId { get; set; }

        public string Otp { get; set; } = string.Empty;
    }
}
