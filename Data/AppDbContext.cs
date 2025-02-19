using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TVOnline.Migrations;
using TVOnline.Models;

namespace TVOnline.Data {
    public class AppDbContext : IdentityDbContext<Users> {
        public AppDbContext(DbContextOptions options) : base(options) {
        }

        public DbSet<Employer> Employers { get; set; }
        public DbSet<InterviewInvitations> InterviewInvitations { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<UserCV> UserCVs { get; set; }
        public DbSet<Payments> Payments { get; set; }
        public DbSet<Templates> Templates { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<PremiumUser> PremiumUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Gọi base method (quan trọng!)

        }
    }
}
