using System.ComponentModel.DataAnnotations;

namespace TVOnline.ViewModels.Chat
{
    public class MarkAsReadViewModel
    {
        [Required]
        public string SenderId { get; set; }

        [Required]
        public string ReceiverId { get; set; }
    }
}