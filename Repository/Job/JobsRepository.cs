using TVOnline.Data;

namespace TVOnline.Repository.Job
{
    public class JobsRepository(AppDbContext context) : IJobsRepository
    {
        private readonly AppDbContext _context = context;
        public Users? FindUsersByEmail(string email) => _context.Users.FirstOrDefault(user => user.Email == email);
    }
}
