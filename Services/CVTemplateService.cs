using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TVOnline.Data;
using TVOnline.Models;

namespace TVOnline.Services
{
    public class CVTemplateService : ICVTemplateService
    {
        private readonly AppDbContext _context;

        public CVTemplateService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CVTemplate>> GetAllTemplatesAsync()
        {
            return await _context.CVTemplates
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<CVTemplate>> GetActiveTemplatesAsync()
        {
            return await _context.CVTemplates
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<CVTemplate> GetTemplateByIdAsync(string id)
        {
            return await _context.CVTemplates.FindAsync(id);
        }

        public async Task<CVTemplate> CreateTemplateAsync(CVTemplate template)
        {
            if (string.IsNullOrEmpty(template.TemplateId))
            {
                template.TemplateId = Guid.NewGuid().ToString();
            }
            
            template.CreatedAt = DateTime.Now;
            
            await _context.CVTemplates.AddAsync(template);
            await _context.SaveChangesAsync();
            
            return template;
        }

        public async Task<CVTemplate> UpdateTemplateAsync(CVTemplate template)
        {
            var existingTemplate = await _context.CVTemplates.FindAsync(template.TemplateId);
            
            if (existingTemplate == null)
            {
                return null;
            }
            
            existingTemplate.Name = template.Name;
            existingTemplate.Description = template.Description;
            existingTemplate.HtmlContent = template.HtmlContent;
            existingTemplate.CssContent = template.CssContent;
            existingTemplate.ThumbnailPath = template.ThumbnailPath;
            existingTemplate.IsActive = template.IsActive;
            existingTemplate.UpdatedAt = DateTime.Now;
            
            _context.CVTemplates.Update(existingTemplate);
            await _context.SaveChangesAsync();
            
            return existingTemplate;
        }

        public async Task<bool> DeleteTemplateAsync(string id)
        {
            var template = await _context.CVTemplates.FindAsync(id);
            
            if (template == null)
            {
                return false;
            }
            
            _context.CVTemplates.Remove(template);
            await _context.SaveChangesAsync();
            
            return true;
        }

        public async Task<bool> ToggleTemplateStatusAsync(string id)
        {
            var template = await _context.CVTemplates.FindAsync(id);
            
            if (template == null)
            {
                return false;
            }
            
            template.IsActive = !template.IsActive;
            template.UpdatedAt = DateTime.Now;
            
            _context.CVTemplates.Update(template);
            await _context.SaveChangesAsync();
            
            return true;
        }
    }
}
