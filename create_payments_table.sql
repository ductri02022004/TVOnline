-- Tạo bảng Payments nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Payments')
BEGIN
    CREATE TABLE [Payments] (
        [PaymentId] nvarchar(450) NOT NULL,
        [PaymentDate] datetime2 NULL,
        [PaymentMethod] nvarchar(max) NULL,
        [UserId] nvarchar(450) NULL,
        [Amount] float NULL,
        [Status] nvarchar(max) NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([PaymentId]),
        CONSTRAINT [FK_Payments_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_Payments_UserId] ON [Payments] ([UserId]);
    
    PRINT 'Đã tạo bảng Payments thành công';
END
ELSE
BEGIN
    PRINT 'Bảng Payments đã tồn tại';
END
