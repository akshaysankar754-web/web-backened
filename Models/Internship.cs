namespace InternshipPortalApi.Models
{
    public class Internship
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public decimal MinimumCGPA { get; set; }

        public int MaximumBacklogs { get; set; }

        public string Department { get; set; } = string.Empty;

        public bool AllowBacklogs { get; set; }

        public DateTime Deadline { get; set; }

        public string CompanyName { get; set; }

        public int NumberOfSeats { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<Application>? Applications { get; set; }
    }
}