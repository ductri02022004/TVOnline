using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

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

        // File attachment (optional)
        public IFormFile? Attachment { get; set; }
    }
}