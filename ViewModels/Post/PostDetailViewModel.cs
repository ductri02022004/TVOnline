using TVOnline.Models;
using TVOnline.Service.DTO;

namespace TVOnline.ViewModels.Post
{
    public class PostDetailViewModel
    {
        public PostResponse Post { get; set; }
        public Users? CurrentUser { get; set; }
    }
}
