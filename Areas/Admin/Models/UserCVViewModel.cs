using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TVOnline.Areas.Admin.Models
{
    public class UserCVViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int CVCount { get; set; }
    }
}
