using Microsoft.EntityFrameworkCore;
using TVOnline.Data;

namespace TVOnline.Repository.Employers
{
    public class EmployerRepository(AppDbContext context) : IEmployerRepository
    {
        public async Task<List<Models.Employers>> GetAllEmployers() => await context.Employers.Include(em => em.City).Include(em => em.User).ToListAsync();

        public async Task<Models.Employers?> GetEmployerById(string employerId) => await context.Employers.FirstOrDefaultAsync(emp => emp.EmployerId == employerId);

        public async Task<List<string>> GetAllEmployerFields()
        {
            var employers = await context.Employers.ToListAsync();
            return employers.Select(e => e.Field).Where(f => !string.IsNullOrEmpty(f)).ToList();
        }

        public async Task<List<Models.Post>> GetPostsByEmployerId(string employerId)
        {
            return await context.Posts
                .Where(p => p.EmployerId == employerId && p.IsActive)
                .Include(p => p.City)
                .Include(p => p.Employer)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}
