-- Cập nhật đường dẫn hình ảnh thumbnail cho các mẫu CV
UPDATE CVTemplates
SET ThumbnailPath = '~/images/cv-templates/professional-cv-template.jpg'
WHERE ThumbnailPath LIKE '/images/cv-templates/%';
