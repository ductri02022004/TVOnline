using TVOnline.Service.DTO;
using TVOnline.Models;

namespace TVOnline.Service.Employers
{
    public interface IEmployersService
    {
        Task<List<EmployerResponse>> GetAllEmployers();
        Task<EmployerResponse?> GetEmployerById(string? employerId);
        Task<Models.Employers> GetEmployerByUserId(string userId);
        Task<bool> UpdateEmployer(Models.Employers employer);
        Task<List<FieldResponse>> GetAllUniqueFields();
        Task<List<EmployerResponse>> SearchEmployers(string companyName, string field, string location);
        Task<List<PostResponse>> GetPostsByEmployerId(string employerId);
    }
}
