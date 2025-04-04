-- Cập nhật đường dẫn hình ảnh thumbnail cho các mẫu CV
-- Sử dụng một đường dẫn chung cho tất cả các mẫu CV
UPDATE CVTemplates
SET ThumbnailPath = '/images/cv-templates/professional-cv-template.jpg'
WHERE ThumbnailPath LIKE '/images/cv-templates/%';
