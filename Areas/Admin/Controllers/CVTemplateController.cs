using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TVOnline.Models;
using TVOnline.Services;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using Microsoft.AspNetCore.Http;

namespace TVOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CVTemplateController : Controller
    {
        private readonly ICVTemplateService _cvTemplateService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CVTemplateController(ICVTemplateService cvTemplateService, IWebHostEnvironment webHostEnvironment)
        {
            _cvTemplateService = cvTemplateService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var templates = await _cvTemplateService.GetAllTemplatesAsync();
            return View(templates);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CVTemplate template, IFormFile thumbnailImage)
        {
            if (ModelState.IsValid)
            {
                // Xử lý tải lên hình ảnh thumbnail nếu có
                if (thumbnailImage != null && thumbnailImage.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "cv-templates");
                    
                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + thumbnailImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await thumbnailImage.CopyToAsync(fileStream);
                    }
                    
                    template.ThumbnailPath = "/images/cv-templates/" + uniqueFileName;
                }
                
                await _cvTemplateService.CreateTemplateAsync(template);
                TempData["SuccessMessage"] = "Mẫu CV đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }
            
            return View(template);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            
            var template = await _cvTemplateService.GetTemplateByIdAsync(id);
            
            if (template == null)
            {
                return NotFound();
            }
            
            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CVTemplate template, IFormFile thumbnailImage)
        {
            if (id != template.TemplateId)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                // Xử lý tải lên hình ảnh thumbnail mới nếu có
                if (thumbnailImage != null && thumbnailImage.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "cv-templates");
                    
                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    
                    // Xóa ảnh cũ nếu có
                    var existingTemplate = await _cvTemplateService.GetTemplateByIdAsync(id);
                    if (existingTemplate != null && !string.IsNullOrEmpty(existingTemplate.ThumbnailPath))
                    {
                        string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingTemplate.ThumbnailPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + thumbnailImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await thumbnailImage.CopyToAsync(fileStream);
                    }
                    
                    template.ThumbnailPath = "/images/cv-templates/" + uniqueFileName;
                }
                
                await _cvTemplateService.UpdateTemplateAsync(template);
                TempData["SuccessMessage"] = "Mẫu CV đã được cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            
            return View(template);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            
            var template = await _cvTemplateService.GetTemplateByIdAsync(id);
            
            if (template == null)
            {
                return NotFound();
            }
            
            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            
            var template = await _cvTemplateService.GetTemplateByIdAsync(id);
            
            if (template == null)
            {
                return NotFound();
            }
            
            // Xóa file ảnh thumbnail nếu có
            if (!string.IsNullOrEmpty(template.ThumbnailPath))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, template.ThumbnailPath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            
            await _cvTemplateService.DeleteTemplateAsync(id);
            TempData["SuccessMessage"] = "Mẫu CV đã được xóa thành công!";
            
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            
            await _cvTemplateService.ToggleTemplateStatusAsync(id);
            TempData["SuccessMessage"] = "Trạng thái mẫu CV đã được cập nhật!";
            
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Preview(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            
            var template = await _cvTemplateService.GetTemplateByIdAsync(id);
            
            if (template == null)
            {
                return NotFound();
            }
            
            return View(template);
        }
    }
}
