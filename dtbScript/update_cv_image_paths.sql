-- Cập nhật đường dẫn hình ảnh thumbnail cho các mẫu CV
UPDATE CVTemplates
SET ThumbnailPath = '~/images/cv-templates/cv-template-default.jpg'
WHERE ThumbnailPath LIKE '%professional-cv-template.jpg';
