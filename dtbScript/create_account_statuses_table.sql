-- Tạo bảng AccountStatuses nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AccountStatuses')
BEGIN
    CREATE TABLE [AccountStatuses] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [IsPremium] bit NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_AccountStatuses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AccountStatuses_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_AccountStatuses_UserId] ON [AccountStatuses] ([UserId]);
END
