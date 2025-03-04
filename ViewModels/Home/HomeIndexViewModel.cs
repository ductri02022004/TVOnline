using TVOnline.Service.DTO;

namespace TVOnline.ViewModels.Home
{
    public class HomeIndexViewModel
    {
        public List<PostResponse> Posts { get; set; }
        public List<CitiesResponse> Locations { get; set; }
    }
}
