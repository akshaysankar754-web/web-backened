namespace InternshipPortalApi.DTOs
{
    public class VerifyEmailDto
    {
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
    }
}
