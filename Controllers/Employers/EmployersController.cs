using Microsoft.AspNetCore.Mvc;

namespace TVOnline.Controllers.Employers
{
    public class EmployersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
