using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static TVOnline.Models.Location;
using TVOnline.Models.Vnpay;
using TVOnline.Models;

namespace TVOnline.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {

        public DbSet<Employers> Employers { get; set; }
        public DbSet<Zone> Zones { get; set; }
        public DbSet<Cities> Cities { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Feedbacks> Feedbacks { get; set; }
        public DbSet<InterviewInvitation> InterviewInvitations { get; set; }
        public DbSet<UserCV> UserCVs { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PremiumUser> PremiumUsers { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<SavedJob> SavedJobs { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Users>()
                .HasMany(u => u.UserCVs)          // Một User có nhiều UserCV  
                .WithOne(cv => cv.Users)         // Một UserCV thuộc về một User  
                .HasForeignKey(cv => cv.UserId); // Chỉ định khóa ngoại UserId trong UserCV  

            // Employers relationships
            modelBuilder.Entity<Employers>()
                .HasOne(e => e.City)
                .WithMany(c => c.Employers)
                .HasForeignKey(e => e.CityId);

            modelBuilder.Entity<Employers>()
                .HasMany(e => e.Posts)
                .WithOne(p => p.Employer)
                .HasForeignKey(p => p.EmployerId);

            // Users - Employers one-to-one relationship
            modelBuilder.Entity<Users>()
                .HasOne(u => u.Employer)
                .WithOne(e => e.User)
                .HasForeignKey<Employers>(e => e.UserId);

            // Cities and Zone relationships
            modelBuilder.Entity<Cities>()
                .HasOne(c => c.Zone)
                .WithMany(z => z.Cities)
                .HasForeignKey(c => c.ZoneId);

            // Feedback relationships
            modelBuilder.Entity<Feedbacks>()
                .HasOne(f => f.User)
                .WithMany(u => u.Feedbacks)
                .HasForeignKey(f => f.UserId);

            // Thiết lập các ràng buộc
            modelBuilder.Entity<Users>()
                .Property(u => u.Email)
                .IsRequired();

            modelBuilder.Entity<Cities>()
                .Property(c => c.CityName)
                .IsRequired();

            modelBuilder.Entity<Zone>()
                .Property(z => z.ZoneName)
                .IsRequired();

            modelBuilder.Entity<Job>()
                .Property(j => j.JobName)
                .IsRequired();

            // Employer properties
            modelBuilder.Entity<Employers>()
                .Property(e => e.CompanyName)
                .IsRequired();

            modelBuilder.Entity<Employers>()
                .Property(e => e.Email)
                .IsRequired();

            modelBuilder.Entity<Employers>()
                .Property(e => e.Description)
                .IsRequired();

            modelBuilder.Entity<Employers>()
                .Property(e => e.Field)
                .IsRequired();

            // cấu hình cho các khóa chính là string
            modelBuilder.Entity<Job>()
                .Property(j => j.JobId)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<Cities>()
                .Property(c => c.CityId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Zone>()
                .Property(z => z.ZoneId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Post>()
                .Property(p => p.PostId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Feedbacks>()
                .Property(f => f.FeedbackId)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<InterviewInvitation>()
                .Property(i => i.InvitationId)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<UserCV>()
                .Property(cv => cv.CvID)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentId)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<PremiumUser>()
                .Property(pu => pu.PremiumUserId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Template>()
                .Property(t => t.TemplateId)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWID()");
        }
    }
}