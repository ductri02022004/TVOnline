using TVOnline.Service.DTO;

namespace TVOnline.ViewModels.Employer
{
    public class CompaniesListViewModel
    {
        public List<EmployerResponse> Employers { get; set; } = new List<EmployerResponse>();
        public List<CitiesResponse> Cities { get; set; } = new List<CitiesResponse>();
        public List<FieldResponse> Fields { get; set; } = new List<FieldResponse>();

        // Search parameters
        public string SearchCompanyName { get; set; }
        public string SearchField { get; set; }
        public string SearchLocation { get; set; }
    }
}
