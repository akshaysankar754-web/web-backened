namespace InternshipPortalApi.DTOs
{
    public class CreateInternshipDto
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public decimal MinimumCGPA { get; set; }

        public int MaximumBacklogs { get; set; }

        public string Department { get; set; } = string.Empty;

        public DateTime Deadline { get; set; }

        public string CompanyName { get; set; }

        public int NumberOfSeats { get; set; }
    }
}