using Microsoft.EntityFrameworkCore;
using TVOnline.Data;

namespace TVOnline.Repository.Employers
{
    public class EmployerRepository(AppDbContext context) : IEmployerRepository
    {
        public async Task<List<Models.Employers>> GetAllEmployers() => await context.Employers.Include(em => em.City).Include(em => em.User).ToListAsync();

        public async Task<Models.Employers?> GetEmployerById(string employerId) => await context.Employers.FirstOrDefaultAsync(emp => emp.EmployerId == employerId);
    }
}
