-- Tạo bảng ChatMessages nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatMessages')
BEGIN
    CREATE TABLE [ChatMessages] (
        [Id] int NOT NULL IDENTITY,
        [SenderId] nvarchar(450) NOT NULL,
        [ReceiverId] nvarchar(450) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        [IsRead] bit NOT NULL,
        CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChatMessages_AspNetUsers_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ChatMessages_AspNetUsers_ReceiverId] FOREIGN KEY ([ReceiverId]) REFERENCES [AspNetUsers] ([Id])
    );

    CREATE INDEX [IX_ChatMessages_SenderId] ON [ChatMessages] ([SenderId]);
    CREATE INDEX [IX_ChatMessages_ReceiverId] ON [ChatMessages] ([ReceiverId]);
    
    PRINT 'Đã tạo bảng ChatMessages thành công';
END
ELSE
BEGIN
    PRINT 'Bảng ChatMessages đã tồn tại';
END
