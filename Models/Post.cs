using System;

namespace TVOnline.Models
{
    public class Post
    {
        public String PostId { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public int EmployerID { get; set; }
        public string ImageUrl { get; set; } 
    }
}