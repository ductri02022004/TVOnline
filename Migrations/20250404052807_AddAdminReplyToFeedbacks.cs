using Microsoft.EntityFrameworkCore.Migrations;

namespace TVOnline.Migrations
{
    public partial class AddAdminReplyToFeedbacks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Kiểm tra và xóa cột Rating cũ nếu tồn tại
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[dbo].[Feedbacks]') 
                    AND name = 'Rating'
                )
                BEGIN
                    ALTER TABLE [dbo].[Feedbacks]
                    DROP COLUMN [Rating]
                END
            ");

            // Thêm cột Rating mới
            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Feedbacks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Xóa bảng Payments cũ nếu tồn tại
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Payments')
                BEGIN
                    DROP TABLE [Payments]
                END
            ");

            // Tạo lại bảng Payments
            migrationBuilder.Sql(@"
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
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Feedbacks");

            // Xóa bảng Payments nếu cần rollback
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Payments')
                BEGIN
                    DROP TABLE [Payments]
                END
            ");
        }
    }
} 