using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TVOnline.Migrations
{
    /// <inheritdoc />
    public partial class dataCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Xóa ràng buộc mặc định của cột Rating nếu tồn tại
            migrationBuilder.Sql(@"
                DECLARE @ConstraintName nvarchar(200)
                SELECT @ConstraintName = dc.name
                FROM sys.default_constraints dc
                JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
                WHERE c.object_id = OBJECT_ID(N'[dbo].[Feedbacks]')
                AND c.name = 'Rating'

                IF @ConstraintName IS NOT NULL
                BEGIN
                    DECLARE @SQL nvarchar(500)
                    SET @SQL = N'ALTER TABLE [dbo].[Feedbacks] DROP CONSTRAINT ' + QUOTENAME(@ConstraintName)
                    EXEC sp_executesql @SQL
                END
            ");

            // Xóa cột Rating cũ nếu tồn tại
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
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[dbo].[Feedbacks]') 
                    AND name = 'Rating'
                )
                BEGIN
                    ALTER TABLE [dbo].[Feedbacks]
                    ADD [Rating] int NOT NULL DEFAULT 0
                END
            ");

            // Cấu hình cho bảng Payments
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
            // Xóa cột Rating
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

            // Xóa cột Status
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
