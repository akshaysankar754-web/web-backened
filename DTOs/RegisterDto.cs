namespace InternshipPortalApi.DTOs
{
    public class RegisterDto
    {
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public decimal CGPA { get; set; }

        public int Backlogs { get; set; }

        public string Department { get; set; } = string.Empty;

        public int PassingYear { get; set; }

        public bool HasBacklogs { get; set; }

        public string CollegeName { get; set; } = string.Empty;
    }
}