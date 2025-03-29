using System.ComponentModel.DataAnnotations;

namespace TVOnline.ViewModels.Chat
{
    public class SendMessageViewModel
    {
        [Required]
        public string SenderId { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        [Required]
        public string Message { get; set; }
    }
}