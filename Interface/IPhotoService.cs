using CloudinaryDotNet.Actions;

namespace TVOnline.Interface {
    public interface IPhotoService {
        Task<ImageUploadResult> AddPhotoAsync(IFormFile file);

        Task<DeletionResult> DeletePhotoAsync(string publicUrl);
    }
}
