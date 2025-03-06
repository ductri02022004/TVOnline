using TVOnline.Service.DTO;
using TVOnline.Models;

namespace TVOnline.Service.Employers
{
    public interface IEmployersService
    {
        Task<List<DTO.EmployerResponse>> GetAllEmployers();
        Task<DTO.EmployerResponse?> GetEmployerById(string? employerId);
        Task<Models.Employers> GetEmployerByUserId(string userId);
        Task<bool> UpdateEmployer(Models.Employers employer);
    }
}
