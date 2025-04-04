-- Kiểm tra xem có admin trong bảng AspNetUsers không
DECLARE @adminId nvarchar(450);
SELECT TOP 1 @adminId = Id FROM AspNetUsers WHERE Email = 'admin@tvonline.com';

-- Kiểm tra xem có người dùng thông thường trong bảng AspNetUsers không
DECLARE @userId1 nvarchar(450);
DECLARE @userId2 nvarchar(450);
SELECT TOP 1 @userId1 = Id FROM AspNetUsers WHERE Email <> 'admin@tvonline.com' ORDER BY Id;
SELECT TOP 1 @userId2 = Id FROM AspNetUsers WHERE Email <> 'admin@tvonline.com' AND Id <> @userId1 ORDER BY Id;

-- In ra thông tin để kiểm tra
PRINT 'Admin ID: ' + ISNULL(@adminId, 'NULL');
PRINT 'User ID 1: ' + ISNULL(@userId1, 'NULL');
PRINT 'User ID 2: ' + ISNULL(@userId2, 'NULL');

-- Thêm tin nhắn mẫu nếu có admin và người dùng
IF @adminId IS NOT NULL AND @userId1 IS NOT NULL
BEGIN
    -- Xóa tin nhắn cũ nếu cần thiết
    -- DELETE FROM ChatMessages WHERE (SenderId = @adminId AND ReceiverId = @userId1) OR (SenderId = @userId1 AND ReceiverId = @adminId);
    
    -- Thêm tin nhắn mẫu giữa admin và user1
    INSERT INTO ChatMessages (SenderId, ReceiverId, Content, Timestamp, IsRead)
    VALUES 
        (@userId1, @adminId, N'Xin chào admin, tôi cần hỗ trợ về tài khoản Premium', DATEADD(HOUR, -2, GETDATE()), 1),
        (@adminId, @userId1, N'Chào bạn, tôi có thể giúp gì cho bạn?', DATEADD(HOUR, -1, GETDATE()), 1),
        (@userId1, @adminId, N'Tôi muốn biết thêm về các tính năng của tài khoản Premium', DATEADD(MINUTE, -30, GETDATE()), 0);
    
    PRINT N'Đã thêm tin nhắn mẫu giữa admin và user1';
END

-- Thêm tin nhắn mẫu giữa admin và user2 nếu có
IF @adminId IS NOT NULL AND @userId2 IS NOT NULL
BEGIN
    -- Xóa tin nhắn cũ nếu cần thiết
    -- DELETE FROM ChatMessages WHERE (SenderId = @adminId AND ReceiverId = @userId2) OR (SenderId = @userId2 AND ReceiverId = @adminId);
    
    -- Thêm tin nhắn mẫu giữa admin và user2
    INSERT INTO ChatMessages (SenderId, ReceiverId, Content, Timestamp, IsRead)
    VALUES 
        (@userId2, @adminId, N'Admin ơi, tôi cần hỗ trợ về CV', DATEADD(HOUR, -3, GETDATE()), 1),
        (@adminId, @userId2, N'Chào bạn, bạn cần hỗ trợ gì về CV?', DATEADD(HOUR, -2, GETDATE()), 1),
        (@userId2, @adminId, N'Tôi không thể tạo CV mới', DATEADD(MINUTE, -45, GETDATE()), 0);
    
    PRINT N'Đã thêm tin nhắn mẫu giữa admin và user2';
END
