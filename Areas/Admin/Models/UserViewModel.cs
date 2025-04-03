namespace TVOnline.Areas.Admin.Models
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = [];
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public EmployerDetailsViewModel? EmployerDetails { get; set; }
        public JobSeekerDetailsViewModel? JobSeekerDetails { get; set; }

        public bool IsLocked => LockoutEnabled && LockoutEnd.HasValue && LockoutEnd.Value > DateTime.Now;
    }

    public class EmployerDetailsViewModel
    {
        public string CompanyName { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class JobSeekerDetailsViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
