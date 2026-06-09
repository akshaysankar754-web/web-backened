namespace InternshipPortalApi.Models
{
    public class Application
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public int InternshipId { get; set; }

        public Internship Internship { get; set; }

        public DateTime AppliedAt { get; set; }

        public string Status { get; set; }
    }
}