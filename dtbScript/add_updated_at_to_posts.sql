-- Thêm cột UpdatedAt vào bảng Posts nếu chưa tồn tại
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Posts' AND COLUMN_NAME = 'UpdatedAt'
)
BEGIN
    ALTER TABLE Posts ADD UpdatedAt datetime2 NULL;
    PRINT 'Đã thêm cột UpdatedAt vào bảng Posts';
END
ELSE
BEGIN
    PRINT 'Cột UpdatedAt đã tồn tại trong bảng Posts';
END
