namespace InternshipPortalApi.Models
{
    public class StudyMaterial
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string FileUrl { get; set; }

        public DateTime UploadedAt { get; set; }

        public int InternshipId { get; set; }

        public Internship? Internship { get; set; }
    }
}