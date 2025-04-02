using TVOnline.Models;
using TVOnline.Service.DTO;

namespace TVOnline.ViewModels.JobsViewModel
{
    public class JobsViewModel
    {
        public List<PostResponse> Posts { get; set; }
        public List<CitiesResponse> Locations { get; set; }

        // Search and filter parameters
        public string? SearchKeyword { get; set; }
        public int? SelectedCityId { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public int? MinExperience { get; set; }
        public int? MaxExperience { get; set; }
    }
}
