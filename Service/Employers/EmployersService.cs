using TVOnline.Repository.Employers;
using TVOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace TVOnline.Service.Employers
{
    public class EmployersService : IEmployersService
    {
        private readonly IEmployerRepository _employerRepository;
        private readonly AppDbContext _context;

        public EmployersService(IEmployerRepository employerRepository, AppDbContext context)
        {
            _employerRepository = employerRepository;
            _context = context;
        }

        public async Task<List<DTO.EmployerResponse>> GetAllEmployers()
        {
            var employers = await _employerRepository.GetAllEmployers();
            return employers.Select(em => em.ToEmployerResponse()).ToList();
        }

        public async Task<DTO.EmployerResponse?> GetEmployerById(string? employerId)
        {
            if (employerId == null) throw new ArgumentNullException(nameof(employerId));
            var employer = await _employerRepository.GetEmployerById(employerId);
            return employer?.ToEmployerResponse();
        }

        public async Task<Models.Employers> GetEmployerByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            var employer = await _context.Employers
                .Include(e => e.City)
                .FirstOrDefaultAsync(e => e.UserId == userId);

            return employer;
        }

        public async Task<bool> UpdateEmployer(Models.Employers employer)
        {
            if (employer == null)
                throw new ArgumentNullException(nameof(employer));

            try
            {
                _context.Employers.Update(employer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
