using TVOnline.Repository.Employers;
using TVOnline.Models;
using Microsoft.EntityFrameworkCore;
using TVOnline.Data;
using TVOnline.Service.DTO;
using System.Text.RegularExpressions;

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

        public async Task<EmployerResponse?> GetEmployerById(string? employerId)
        {
            if (employerId == null) throw new ArgumentNullException(nameof(employerId));
            var employer = await _employerRepository.GetEmployerById(employerId);

            if (employer == null) return null;

            var response = employer.ToEmployerResponse();
            response.Posts = await GetPostsByEmployerId(employerId);

            return response;
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

        public async Task<List<FieldResponse>> GetAllUniqueFields()
        {
            var allFields = await _employerRepository.GetAllEmployerFields();
            var uniqueFields = new HashSet<string>();

            foreach (var field in allFields)
            {
                if (string.IsNullOrEmpty(field)) continue;

                var fieldArray = field.Split([", "], StringSplitOptions.RemoveEmptyEntries);

                foreach (var singleField in fieldArray)
                {
                    if (!string.IsNullOrWhiteSpace(singleField))
                    {
                        uniqueFields.Add(singleField.Trim());
                    }
                }
            }

            return [.. uniqueFields
                .OrderBy(f => f)
                .Select(field => new FieldResponse(field))];
        }

        public async Task<List<DTO.EmployerResponse>> SearchEmployers(string companyName, string field, string location)
        {
            var employers = await _employerRepository.GetAllEmployers();
            var results = employers.AsQueryable();

            // Filter by company name
            if (!string.IsNullOrWhiteSpace(companyName))
            {
                results = results.Where(e => e.CompanyName.Contains(companyName, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by field
            if (!string.IsNullOrWhiteSpace(field))
            {
                results = results.Where(e =>
                    e.Field != null &&
                    (e.Field.Equals(field, StringComparison.OrdinalIgnoreCase) ||
                     e.Field.Contains($", {field}", StringComparison.OrdinalIgnoreCase) ||
                     e.Field.Contains($"{field}, ", StringComparison.OrdinalIgnoreCase)));
            }

            // Filter by location
            if (!string.IsNullOrWhiteSpace(location))
            {
                results = results.Where(e => e.City != null && e.City.CityName.Contains(location, StringComparison.OrdinalIgnoreCase));
            }

            return [.. results.Select(e => e.ToEmployerResponse())];
        }

        public async Task<List<DTO.PostResponse>> GetPostsByEmployerId(string employerId)
        {
            var posts = await _employerRepository.GetPostsByEmployerId(employerId);
            return posts.Select(p => p.ToPostResponse()).ToList();
        }
    }
}
