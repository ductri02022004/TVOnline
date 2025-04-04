using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TVOnline.Services;

namespace TVOnline.Areas.Premium.Controllers
{
    [Area("Premium")]
    [Authorize(Policy = "PremiumUser")]
    public class TemplateController : Controller
    {
        private readonly ICVTemplateService _cvTemplateService;

        public TemplateController(ICVTemplateService cvTemplateService)
        {
            _cvTemplateService = cvTemplateService;
        }

        // Chuyển hướng đến CVController/Index
        public IActionResult Index()
        {
            return RedirectToAction("Index", "CV", new { area = "Premium" });
        }

        // Chuyển hướng đến CVController/Preview
        public IActionResult Preview(string id)
        {
            return RedirectToAction("Preview", "CV", new { area = "Premium", id = id });
        }

        // Chuyển hướng đến CVController/Create
        public IActionResult Create(string templateId)
        {
            return RedirectToAction("Create", "CV", new { area = "Premium", templateId = templateId });
        }

        // Chuyển hướng đến CVController/MyCV
        public IActionResult MyCV()
        {
            return RedirectToAction("MyCV", "CV", new { area = "Premium" });
        }

        // Chuyển hướng đến CVController/View
        public IActionResult View(string id)
        {
            return RedirectToAction("View", "CV", new { area = "Premium", id = id });
        }
    }
}
