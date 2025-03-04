namespace TVOnline.Service.Employers
{
    public interface IEmployersService
    {
        Task<List<EmployerResponse>> GetAllEmployers();
        Task<EmployerResponse?> GetEmployerById(string? employerId);
    }
}
