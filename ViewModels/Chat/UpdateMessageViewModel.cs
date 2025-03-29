using System.ComponentModel.DataAnnotations;

namespace TVOnline.ViewModels.Chat
{
    public class UpdateMessageViewModel
    {
        [Required]
        public string Content { get; set; }
    }
}