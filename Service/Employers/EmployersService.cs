namespace TVOnline.Service.Employers
{
    public class EmployersService(IEmployerRepository employerRepository) : IEmployersService
    {
        public async Task<List<EmployerResponse>> GetAllEmployers()
        {
            var employers = await employerRepository.GetAllEmployers();
            return employers.Select(em => em.ToEmployerResponse()).ToList();
        }

        public async Task<EmployerResponse?> GetEmployerById(string? employerId)
        {
            if (employerId == null) throw new ArgumentNullException(nameof(employerId));
            var employer = await employerRepository.GetEmployerById(employerId);
            return employer?.ToEmployerResponse();
        }
    }
}
