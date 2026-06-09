namespace InternshipPortalApi.DTOs
{
    public class ProfileDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal CGPA { get; set; }
        public int Backlogs { get; set; }
        public string Department { get; set; } = string.Empty;
        public int PassingYear { get; set; }
        public string CollegeName { get; set; } = string.Empty;
        public bool HasBacklogs { get; set; }
    }
}