using TVOnline.Service.DTO;

namespace TVOnline.ViewModels.Employer
{
    public class CompaniesListViewModel
    {
        public List<EmployerResponse> Employers { get; set; }
        public List<CitiesResponse> Cities { get; set; }
    }
}
