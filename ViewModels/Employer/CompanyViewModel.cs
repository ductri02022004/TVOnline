using TVOnline.Service.DTO;
using TVOnline.Service.Location;

namespace TVOnline.ViewModels.Employer
{
    public class CompanyViewModel
    {
        public string EmployerId { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public string Field { get; set; }
        public string LogoUrl { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Danh sách cho dropdown (nếu cần)
        public List<CitiesResponse> Cities { get; set; }
    }
    
    // Extension method để chuyển đổi từ EmployerResponse sang CompanyViewModel
    public static class CompanyViewModelExtensions
    {
        public static CompanyViewModel ToCompanyViewModel(this EmployerResponse response)
        {
            return new CompanyViewModel
            {
                EmployerId = response.EmployerId,
                CompanyName = response.CompanyName,
                Email = response.Email,
                Description = response.Description,
                Field = response.Field,
                LogoUrl = response.LogoURL,
                Address = response.CityName,
                Phone = "", // Cần bổ sung thông tin này từ model nếu có
                CreatedAt = response.CreatedAt,
                Cities = null // Cần thiết lập nếu cần
            };
        }
    }
}
