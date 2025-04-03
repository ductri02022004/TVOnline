using System.Collections.Generic;
using System.Threading.Tasks;
using TVOnline.Models;

namespace TVOnline.Services
{
    public interface ICVTemplateService
    {
        Task<List<CVTemplate>> GetAllTemplatesAsync();
        Task<List<CVTemplate>> GetActiveTemplatesAsync();
        Task<CVTemplate> GetTemplateByIdAsync(string id);
        Task<CVTemplate> CreateTemplateAsync(CVTemplate template);
        Task<CVTemplate> UpdateTemplateAsync(CVTemplate template);
        Task<bool> DeleteTemplateAsync(string id);
        Task<bool> ToggleTemplateStatusAsync(string id);
    }
}
