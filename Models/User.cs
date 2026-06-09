namespace InternshipPortalApi.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public decimal CGPA { get; set; }

        public bool HasBacklogs { get; set; }

        public string CollegeName { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public int Backlogs { get; set; }

        public int PassingYear { get; set; }

        public bool IsVerified { get; set; }

        public string OtpCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}