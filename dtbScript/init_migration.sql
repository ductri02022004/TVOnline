IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FullName] nvarchar(max) NULL,
    [UserCity] nvarchar(max) NULL,
    [UserJob] nvarchar(max) NULL,
    [Dob] datetime2 NULL,
    [EmployerId] nvarchar(max) NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NOT NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Jobs] (
    [JobId] nvarchar(450) NOT NULL DEFAULT (NEWID()),
    [JobName] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Jobs] PRIMARY KEY ([JobId])
);
GO

CREATE TABLE [Zones] (
    [ZoneId] int NOT NULL IDENTITY,
    [ZoneName] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Zones] PRIMARY KEY ([ZoneId])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [PaymentInformationModel] (
    [OrderId] int NOT NULL IDENTITY,
    [OrderType] nvarchar(max) NOT NULL,
    [Amount] float NULL,
    [OrderDescription] nvarchar(max) NULL,
    [Name] nvarchar(max) NULL,
    [UsersId] nvarchar(450) NULL,
    CONSTRAINT [PK_PaymentInformationModel] PRIMARY KEY ([OrderId]),
    CONSTRAINT [FK_PaymentInformationModel_AspNetUsers_UsersId] FOREIGN KEY ([UsersId]) REFERENCES [AspNetUsers] ([Id])
);
GO

CREATE TABLE [Payments] (
    [PaymentId] nvarchar(450) NOT NULL DEFAULT (NEWID()),
    [PaymentDate] datetime2 NULL,
    [PaymentMethod] nvarchar(max) NULL,
    [UserId] nvarchar(450) NULL,
    [Amount] float NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY ([PaymentId]),
    CONSTRAINT [FK_Payments_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id])
);
GO

CREATE TABLE [PremiumUsers] (
    [PremiumUserId] nvarchar(450) NOT NULL,
    [UserId] nvarchar(450) NULL,
    CONSTRAINT [PK_PremiumUsers] PRIMARY KEY ([PremiumUserId]),
    CONSTRAINT [FK_PremiumUsers_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id])
);
GO

CREATE TABLE [Cities] (
    [CityId] int NOT NULL IDENTITY,
    [CityName] nvarchar(max) NOT NULL,
    [ZoneId] int NOT NULL,
    CONSTRAINT [PK_Cities] PRIMARY KEY ([CityId]),
    CONSTRAINT [FK_Cities_Zones_ZoneId] FOREIGN KEY ([ZoneId]) REFERENCES [Zones] ([ZoneId]) ON DELETE CASCADE
);
GO

CREATE TABLE [Templates] (
    [TemplateId] nvarchar(450) NOT NULL DEFAULT (NEWID()),
    [TemplateName] nvarchar(max) NULL,
    [TemplateFileURL] nvarchar(max) NULL,
    [PremiumUserId] nvarchar(450) NULL,
    CONSTRAINT [PK_Templates] PRIMARY KEY ([TemplateId]),
    CONSTRAINT [FK_Templates_PremiumUsers_PremiumUserId] FOREIGN KEY ([PremiumUserId]) REFERENCES [PremiumUsers] ([PremiumUserId])
);
GO

CREATE TABLE [Employers] (
    [EmployerId] nvarchar(450) NOT NULL,
    [UserId] nvarchar(450) NULL,
    [Email] nvarchar(max) NOT NULL,
    [CompanyName] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Field] nvarchar(max) NOT NULL,
    [LogoURL] nvarchar(max) NULL,
    [Website] nvarchar(max) NULL,
    [CityId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ZoneId] int NULL,
    CONSTRAINT [PK_Employers] PRIMARY KEY ([EmployerId]),
    CONSTRAINT [FK_Employers_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_Employers_Cities_CityId] FOREIGN KEY ([CityId]) REFERENCES [Cities] ([CityId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Employers_Zones_ZoneId] FOREIGN KEY ([ZoneId]) REFERENCES [Zones] ([ZoneId])
);
GO

CREATE TABLE [Feedbacks] (
    [FeedbackId] nvarchar(450) NOT NULL DEFAULT (NEWID()),
    [Content] nvarchar(max) NULL,
    [Date] datetime2 NOT NULL,
    [UserId] nvarchar(450) NULL,
    [EmployerId] nvarchar(450) NULL,
    CONSTRAINT [PK_Feedbacks] PRIMARY KEY ([FeedbackId]),
    CONSTRAINT [FK_Feedbacks_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_Feedbacks_Employers_EmployerId] FOREIGN KEY ([EmployerId]) REFERENCES [Employers] ([EmployerId])
);
GO

CREATE TABLE [InterviewInvitations] (
    [InvitationId] nvarchar(450) NOT NULL DEFAULT (NEWID()),
    [InvitationDate] datetime2 NOT NULL,
    [UserId] nvarchar(450) NULL,
    [EmployerId] nvarchar(450) NULL,
    CONSTRAINT [PK_InterviewInvitations] PRIMARY KEY ([InvitationId]),
    CONSTRAINT [FK_InterviewInvitations_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_InterviewInvitations_Employers_EmployerId] FOREIGN KEY ([EmployerId]) REFERENCES [Employers] ([EmployerId])
);
GO

CREATE TABLE [Posts] (
    [PostId] nvarchar(450) NOT NULL,
    [EmployerId] nvarchar(450) NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Benefits] nvarchar(max) NOT NULL,
    [Salary] decimal(18,2) NOT NULL,
    [Position] nvarchar(max) NOT NULL,
    [Experience] nvarchar(max) NOT NULL,
    [CityId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [Requirements] nvarchar(max) NOT NULL,
    [JobType] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Posts] PRIMARY KEY ([PostId]),
    CONSTRAINT [FK_Posts_Cities_CityId] FOREIGN KEY ([CityId]) REFERENCES [Cities] ([CityId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Posts_Employers_EmployerId] FOREIGN KEY ([EmployerId]) REFERENCES [Employers] ([EmployerId])
);
GO

CREATE TABLE [SavedJobs] (
    [SavedJobId] nvarchar(450) NOT NULL,
    [PostId] nvarchar(450) NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [SavedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_SavedJobs] PRIMARY KEY ([SavedJobId]),
    CONSTRAINT [FK_SavedJobs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SavedJobs_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [Posts] ([PostId]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserCVs] (
    [CvID] nvarchar(450) NOT NULL DEFAULT (NEWID()),
    [UserId] nvarchar(450) NULL,
    [CVFileUrl] nvarchar(max) NULL,
    [CVStatus] nvarchar(max) NULL,
    [ApplicationDate] datetime2 NOT NULL,
    [PostId] nvarchar(450) NULL,
    [AppliedDate] datetime2 NOT NULL,
    [EmployerNotes] nvarchar(max) NULL,
    CONSTRAINT [PK_UserCVs] PRIMARY KEY ([CvID]),
    CONSTRAINT [FK_UserCVs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_UserCVs_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [Posts] ([PostId])
);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_Cities_ZoneId] ON [Cities] ([ZoneId]);
GO

CREATE INDEX [IX_Employers_CityId] ON [Employers] ([CityId]);
GO

CREATE UNIQUE INDEX [IX_Employers_UserId] ON [Employers] ([UserId]) WHERE [UserId] IS NOT NULL;
GO

CREATE INDEX [IX_Employers_ZoneId] ON [Employers] ([ZoneId]);
GO

CREATE INDEX [IX_Feedbacks_EmployerId] ON [Feedbacks] ([EmployerId]);
GO

CREATE INDEX [IX_Feedbacks_UserId] ON [Feedbacks] ([UserId]);
GO

CREATE INDEX [IX_InterviewInvitations_EmployerId] ON [InterviewInvitations] ([EmployerId]);
GO

CREATE INDEX [IX_InterviewInvitations_UserId] ON [InterviewInvitations] ([UserId]);
GO

CREATE INDEX [IX_PaymentInformationModel_UsersId] ON [PaymentInformationModel] ([UsersId]);
GO

CREATE INDEX [IX_Payments_UserId] ON [Payments] ([UserId]);
GO

CREATE INDEX [IX_Posts_CityId] ON [Posts] ([CityId]);
GO

CREATE INDEX [IX_Posts_EmployerId] ON [Posts] ([EmployerId]);
GO

CREATE UNIQUE INDEX [IX_PremiumUsers_UserId] ON [PremiumUsers] ([UserId]) WHERE [UserId] IS NOT NULL;
GO

CREATE INDEX [IX_SavedJobs_PostId] ON [SavedJobs] ([PostId]);
GO

CREATE INDEX [IX_SavedJobs_UserId] ON [SavedJobs] ([UserId]);
GO

CREATE INDEX [IX_Templates_PremiumUserId] ON [Templates] ([PremiumUserId]);
GO

CREATE INDEX [IX_UserCVs_PostId] ON [UserCVs] ([PostId]);
GO

CREATE INDEX [IX_UserCVs_UserId] ON [UserCVs] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250306050802_init', N'8.0.12');
GO

COMMIT;
GO

