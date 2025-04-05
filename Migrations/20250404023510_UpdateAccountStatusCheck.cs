using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVOnline.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAccountStatusCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "AccountStatuses",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            // Kiểm tra xem bảng ChatMessages đã tồn tại chưa
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatMessages')
                BEGIN
                    CREATE TABLE [ChatMessages] (
                        [Id] int NOT NULL IDENTITY,
                        [SenderId] nvarchar(max) NOT NULL,
                        [ReceiverId] nvarchar(max) NOT NULL,
                        [Content] nvarchar(max) NOT NULL,
                        [Timestamp] datetime2 NOT NULL,
                        [IsRead] bit NOT NULL,
                        CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([Id])
                    );
                END
            ");

            // Thêm cột Status vào bảng Payments nếu chưa tồn tại
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[dbo].[Payments]') 
                    AND name = 'Status'
                )
                BEGIN
                    ALTER TABLE [dbo].[Payments]
                    ADD [Status] nvarchar(max) NULL
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Không xóa bảng ChatMessages trong phương thức Down để tránh mất dữ liệu
            // migrationBuilder.DropTable(
            //    name: "ChatMessages");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "AccountStatuses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            // Xóa cột Status nếu cần rollback
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[dbo].[Payments]') 
                    AND name = 'Status'
                )
                BEGIN
                    ALTER TABLE [dbo].[Payments]
                    DROP COLUMN [Status]
                END
            ");
        }
    }
}
