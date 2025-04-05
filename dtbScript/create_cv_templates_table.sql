-- Tạo bảng CVTemplates nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CVTemplates')
BEGIN
    CREATE TABLE [CVTemplates] (
        [TemplateId] nvarchar(450) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [HtmlContent] nvarchar(max) NOT NULL,
        [CssContent] nvarchar(max) NOT NULL,
        [ThumbnailPath] nvarchar(255) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_CVTemplates] PRIMARY KEY ([TemplateId])
    );
    PRINT 'Đã tạo bảng CVTemplates thành công';
END
ELSE
BEGIN
    PRINT 'Bảng CVTemplates đã tồn tại';
END
