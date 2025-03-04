using TVOnline.Service.DTO;

namespace TVOnline.ViewModels.JobsViewModel
{
    public class JobsViewModel
    {
        public List<PostResponse> Posts { get; set; }
        public List<CitiesResponse> Locations { get; set; }
    }
}
