using Microsoft.EntityFrameworkCore;

using InternshipPortalApi.Models;

namespace InternshipPortalApi.Data
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options
        ) : base(options)
        {
        }


        public DbSet<User> Users { get; set; }

        public DbSet<Internship> Internships { get; set; }

        public DbSet<Application> Applications { get; set; }

        public DbSet<Feedback> Feedbacks { get; set; }

        public DbSet<StudyMaterial> StudyMaterials { get; set; }

        public DbSet<FeedbackSetting> FeedbackSettings { get; set; }

        public DbSet<EmailOtp> EmailOtps { get; set; }

    }
}